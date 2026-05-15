using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Data.SqlTypes;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace capybara;

/// <summary>
/// Segunda escena de juego del Mundo 1. Carga y gestiona el nivel 2 a partir
/// de su archivo CSV, controla al personaje, detecta colisiones con tiles
/// especiales y gestiona las transiciones al nivel 3 (<see cref="GameScene3"/>),
/// a la pantalla de victoria (<see cref="WinScene"/>) o a game over según el
/// estado del <see cref="VidaManager"/>.
/// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
/// </summary>
public class GameScene2 : IScene
{
    // Añade estas dos líneas:
private bool sobreMuerte = false;
private bool sobreMeta = false;
    /// <summary>Referencia al gestor de escenas para poder apilar nuevas escenas.</summary>
    private SceneManager _sceneManager;

    /// <summary>
    /// Diccionario que representa el tilemap del nivel. La clave es la posición
    /// del tile en coordenadas de cuadrícula (columna, fila) y el valor es el
    /// identificador numérico del tile en el tilesheet.
    /// </summary>
    private Dictionary<Vector2, int> tilemap;

    /// <summary>
    /// Lista de rectángulos fuente que mapean cada identificador de tile
    /// a su posición correspondiente dentro del <see cref="textureAtlas"/>.
    /// El índice de la lista equivale al valor del tile menos 1.
    /// </summary>
    private List<Rectangle> texturas;

    /// <summary>Textura atlas (tilesheet) que contiene todos los sprites de tiles del nivel.</summary>
    private Texture2D textureAtlas;

    /// <summary>Sprite del jugador con físicas y animaciones integradas.</summary>
    private MovedSprite personaje;

    /// <summary>
    /// Lista de sprites activos en la escena, excluyendo al personaje.
    /// Se usa para gestionar elementos adicionales como coleccionables o decoraciones dinámicas.
    /// </summary>
    private List<Sprite> sprites;

    /// <summary>
    /// Textura de 1×1 píxel blanco usada para dibujar el bounding box de depuración
    /// del personaje en rojo semitransparente.
    /// </summary>
    private Texture2D pixel;
    /// <summary>
    /// Acumulador de tiempo que el jugador lleva sobre un tile de muerte (50).
    /// Solo se pierde la vida cuando supera <see cref="DelayMuerte"/>, dando
    /// margen para saltar y esquivar.
    /// </summary>
    private float _timerMuerte = 0f;
    private const float DelayMuerte = 0.6f;

    /// <summary>
    /// Acumulador de tiempo que el jugador lleva sobre un tile de meta/victoria (99/100).
    /// Solo se transiciona cuando supera <see cref="DelayMeta"/>, evitando
    /// transiciones accidentales por rozar el tile.
    /// </summary>
    private float _timerMeta = 0f;
    private const float DelayMeta = 0.3f;


    /// <summary>Factor de escala visual aplicado al sprite del personaje.</summary>
    private float escala = 1.0f;

    /// <summary>Velocidad de desplazamiento horizontal del personaje en píxeles por frame.</summary>
    private float velocidad = 4f;

    /// <summary>Aceleración gravitacional aplicada al personaje cada frame en píxeles por frame².</summary>
    private float gravedad = 0.5f;

    /// <summary>Fuerza de salto inicial del personaje (negativa porque Y crece hacia abajo).</summary>
    private float fuerza = -8f;

    /// <summary>Estado del teclado en el frame anterior, necesario para detectar pulsaciones únicas.</summary>
    private KeyboardState teclaanterior;

    /// <summary>Gestor de contenido usado para cargar texturas del nivel.</summary>
    private ContentManager Content;

    /// <summary>Dispositivo gráfico necesario para crear texturas procedurales.</summary>
    private GraphicsDevice _graphicsDevice;

    /// <summary>Textura de fondo del nivel 2 (comparte fondo con el nivel 1 del Mundo 1).</summary>
    private Texture2D _fondo;

    /// <summary>
    /// Canción de fondo que suena durante los niveles de juego.
    /// Se reproduce en bucle desde <see cref="LoadContent"/>.
    /// </summary>
    private Song musica_nivel;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GameScene2"/>.
    /// Suscribe los callbacks a los eventos del <see cref="VidaManager"/>
    /// para reaccionar ante pérdida de vida y game over.
    /// </summary>
    /// <param name="sm">Gestor de escenas del juego.</param>
    /// <param name="content">Gestor de contenido para la carga de assets.</param>
    /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
    public GameScene2(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        this.Content = content;
        this._graphicsDevice = gd;

        VidaManager.OnPerderVida += Respawn;
        VidaManager.OnGameOver += GameOver;
    }

    /// <summary>
    /// Callback suscrito a <see cref="VidaManager.OnPerderVida"/>.
    /// Reposiciona al personaje en el punto de spawn inicial del nivel
    /// y anula su velocidad para evitar inercia residual.
    /// </summary>
    private void Respawn()
    {
        personaje.position = new Vector2(100, 90);
        personaje.velocity = Vector2.Zero;
    }

    /// <summary>
    /// Callback suscrito a <see cref="VidaManager.OnGameOver"/>.
    /// Se desuscribe de ambos eventos del <see cref="VidaManager"/> para evitar
    /// referencias colgadas, borra el guardado, limpia el <see cref="CollisionManager"/>
    /// y apila la escena <see cref="GameOverScene"/>.
    /// </summary>
    private void GameOver()
    {
        VidaManager.OnPerderVida -= Respawn;
        VidaManager.OnGameOver -= GameOver;

        // El save NO se borra al morir: el jugador debe poder usar Continue
        // desde el último checkpoint. BorrarSave solo se llama al iniciar
        // partida nueva (NewGame) o al completar el juego (WinScene).

        CollisionManager.Clear();
        GameOverScene gameOver = new GameOverScene(_sceneManager, Content, _graphicsDevice, this.GetType());
        gameOver.LoadContent();
        _sceneManager.AddScene(gameOver);
    }

    /// <summary>
    /// Carga todos los recursos del nivel 2: tilemap desde CSV, fondo, tilesheet,
    /// sprites del personaje y sus animaciones. Registra los colisionadores del
    /// personaje y de todos los tiles sólidos en el <see cref="CollisionManager"/>.
    /// <para>
    /// Tiles excluidos de la generación de colisionadores:
    /// <list type="bullet">
    ///   <item><description>18 — tile decorativo sin colisión física.</description></item>
    ///   <item><description>50 — tile de muerte (trigger de pérdida de vida).</description></item>
    ///   <item><description>99 — tile de meta (trigger de transición a <see cref="GameScene3"/>).</description></item>
    ///   <item><description>100 — tile de victoria (trigger de transición a <see cref="WinScene"/>).</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public void LoadContent()
    {
        tilemap = CargarMapa("Content/nivel2.csv");
        _fondo = Content.Load<Texture2D>("fondo_nivel1");
        textureAtlas = Content.Load<Texture2D>("tilesheet");
        Texture2D texturecapibara = Content.Load<Texture2D>("personaje_basico");
        Texture2D texAnimacion = Content.Load<Texture2D>("animacion_burbuja");
        Texture2D texSalida = Content.Load<Texture2D>("animacion_romper_burbuja");
        SoundEffect sonidoSalto = Content.Load<SoundEffect>("sonido_salto");
        SoundEffect sonidoBurbuja = Content.Load<SoundEffect>("sonido_entrar_burbuja");
        SoundEffect sonidoFueraburbuja = Content.Load<SoundEffect>("sonido_salir_burbuja");

        personaje = new MovedSprite(texturecapibara, new Vector2(100, 90), escala, velocidad,
            texAnimacion, texSalida, sonidoSalto, sonidoBurbuja, sonidoFueraburbuja);
        sprites = new List<Sprite> { personaje };

        CollisionManager.AddCollider(personaje.Collider);

        texturas = new()
        {
            new Rectangle(0,32,32,32),    //1
            new Rectangle(32,32,32,32),   //2
            new Rectangle(64,32,32,32),   //3
            new Rectangle(96,32,32,32),   //4
            new Rectangle(128,32,32,32),  //5
            new Rectangle(160,32,32,32),  //6
            new Rectangle(192,32,32,32),  //7
            new Rectangle(0,64,32,32),    //8
            new Rectangle(32,64,32,32),   //9
            new Rectangle(64,64,32,32),   //10
            new Rectangle(96,64,32,32),   //11
            new Rectangle(128,64,32,32),  //12
            new Rectangle(160,64,32,32),  //13
            new Rectangle(0,96,32,32),    //14
            new Rectangle(32,96,32,32),   //15
            new Rectangle(64,96,32,32),   //16
            new Rectangle(96,96,32,32),   //17
            new Rectangle(128,96,32,32),  //18
            new Rectangle(160,96,32,32),  //19
            new Rectangle(96,128,32,32),  //20
            new Rectangle(128,128,32,32), //21
            new Rectangle(160,128,32,32)  //22
        };

        bool sobreMuerte = false;
        bool sobreMeta   = false;
        int tileSize = 60;
        foreach (var item in tilemap)
        {
            if (item.Value !=18 && item.Value != 99 && item.Value != 50 && item.Value != 100)
            {
                BoxCollider bloque = new BoxCollider(
                    new Vector2(item.Key.X * tileSize, item.Key.Y * tileSize),
                    tileSize, tileSize);
                CollisionManager.AddCollider(bloque);
            }
        }

        pixel = new Texture2D(_graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        musica_nivel = Content.Load<Song>("musica_niveles");
    }

    /// <summary>
    /// Actualiza la lógica del nivel cada frame: sincroniza el colisionador del
    /// personaje, actualiza el <see cref="CollisionManager"/>, procesa la entrada
    /// del jugador y comprueba colisiones con tiles especiales.
    /// <para>
    /// Tiles especiales gestionados:
    /// <list type="bullet">
    ///   <item><description>50 — al tocarlo se invoca <see cref="VidaManager.PerderVida"/>.</description></item>
    ///   <item><description>99 — desuscribe eventos, limpia colisiones y carga <see cref="GameScene3"/>.</description></item>
    ///   <item><description>100 — desuscribe eventos, limpia colisiones y carga <see cref="WinScene"/>.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState tecladoActual = Keyboard.GetState();

        if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != musica_nivel)
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(musica_nivel);
        }

        Rectangle playerRect = personaje.Rect;
        personaje.Collider.Position = new Vector2(playerRect.X, playerRect.Y);
        personaje.Collider.Width = playerRect.Width;
        personaje.Collider.Height = playerRect.Height;

        CollisionManager.Update();
        personaje.Update(tecladoActual, teclaanterior, gravedad, fuerza, gameTime);

        List<Sprite> killist = new();
        foreach (var sprite in sprites)
        {
            if (sprite != personaje && sprite is ScaledSprite escalado && escalado.Rect.Intersects(personaje.Rect))
                killist.Add(sprite);
        }
        foreach (var sprite in killist) sprites.Remove(sprite);

        int tileSize = 60;
        foreach (var item in tilemap)
        {
            if (item.Value == 50)
            {
                Rectangle tileKill = new Rectangle(
                    (int)item.Key.X * tileSize,
                    (int)item.Key.Y * tileSize,
                    tileSize, tileSize);

                if (personaje.Rect.Intersects(tileKill))
                {
                    sobreMuerte = true;
                    _timerMuerte += dt;
                    if (_timerMuerte >= DelayMuerte)
                    {
                        _timerMuerte = 0f;
                        VidaManager.PerderVida();
                        break;
                    }
                } else { _timerMuerte = 0f; }
            }

            if (item.Value == 99)
            {
                Rectangle tileMeta = new Rectangle(
                    (int)item.Key.X * tileSize,
                    (int)item.Key.Y * tileSize,
                    tileSize, tileSize);

                if (personaje.Rect.Intersects(tileMeta))
                {
                    VidaManager.OnPerderVida -= Respawn;
                    VidaManager.OnGameOver -= GameOver;

                    CollisionManager.Clear();
                    GameScene3 nivel3 = new GameScene3(_sceneManager, Content, _graphicsDevice);
                    nivel3.LoadContent();
                    _sceneManager.AddScene(nivel3);
                    return;
                }
            }

            if (item.Value == 100)
            {
                Rectangle tileWin = new Rectangle(
                    (int)item.Key.X * 60,
                    (int)item.Key.Y * 60,
                    60, 60);

                if (personaje.Rect.Intersects(tileWin))
                {
                    sobreMeta = true;
                    _timerMeta += dt;
                    if (_timerMeta >= DelayMeta)
                    {
                        _timerMeta = 0f;
                    VidaManager.OnPerderVida -= Respawn;
                    VidaManager.OnGameOver -= GameOver;

                    CollisionManager.Clear();
                    WinScene victoria = new WinScene(_sceneManager, Content, _graphicsDevice);
                    victoria.LoadContent();
                    _sceneManager.AddScene(victoria);
                    return;
                    }
                } else { _timerMeta = 0f; }
            }
        }

        teclaanterior = tecladoActual;
    }

    /// <summary>
    /// Dibuja todos los elementos visuales del nivel en el orden correcto:
    /// fondo, tiles del tilemap (omitiendo los tiles especiales sin textura),
    /// el personaje con su animación activa (burbuja, salida de burbuja o idle)
    /// y el bounding box de depuración en rojo semitransparente.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = 60;
        spriteBatch.Draw(_fondo, new Rectangle(0, 0, 1280, 720), Color.White);

        foreach (var item in tilemap)
        {
            if (item.Value == 99 || item.Value == 50 || item.Value == 100 || item.Value == 18) continue;
            Rectangle dest = new((int)item.Key.X * tileSize, (int)item.Key.Y * tileSize, tileSize, tileSize);
            spriteBatch.Draw(textureAtlas, dest, texturas[item.Value - 1], Color.White);
        }

        if (personaje.EnEstadoS || personaje.SaliendoDeS)
        {
            Texture2D texAUsar = personaje.EnEstadoS ? personaje.TexEspecial : personaje.TexSalida;
            int totalFrames = personaje.EnEstadoS ? personaje.TotalFramesS : personaje.TotalFramesSalida;

            int anchoFrame = texAUsar.Width / totalFrames;
            int altoFrame = texAUsar.Height;
            Rectangle fuente = new Rectangle(personaje.FrameActualS * anchoFrame, 0, anchoFrame, altoFrame);
            float escalaAjustadaX = (float)personaje.texture.Width / anchoFrame;
            float escalaAjustadaY = (float)personaje.texture.Height / altoFrame;
            Vector2 escalaFinal = new Vector2(escalaAjustadaX * escala, escalaAjustadaY * escala);
            Vector2 origen = new Vector2(anchoFrame / 2f, altoFrame / 2f);

            spriteBatch.Draw(texAUsar, personaje.position, fuente, Color.White, 0f, origen, escalaFinal, personaje.efecto, 0f);
        }
        else
        {
            spriteBatch.Draw(personaje.texture, personaje.position, null, Color.White, 0f,
                new Vector2(personaje.texture.Width / 2f, personaje.texture.Height / 2f),
                escala, personaje.efecto, 0f);
        }

       
    }

    /// <summary>
    /// Lee un archivo CSV de tilemap y lo convierte en un diccionario de tiles.
    /// Cada fila del CSV corresponde a una fila de tiles y cada valor separado
    /// por coma a una columna. Solo se almacenan los tiles con valor mayor que 0.
    /// </summary>
    /// <param name="ruta">Ruta relativa al archivo CSV del nivel (p. ej. <c>"Content/nivel2.csv"</c>).</param>
    /// <returns>
    /// Diccionario donde la clave es la posición en cuadrícula <c>(columna, fila)</c>
    /// y el valor es el identificador numérico del tile. Devuelve un diccionario
    /// vacío si el archivo no existe.
    /// </returns>
    private Dictionary<Vector2, int> CargarMapa(string ruta)
    {
        Dictionary<Vector2, int> resultado = new();
        if (!File.Exists(ruta)) return resultado;
        using StreamReader lector = new(ruta);
        int y = 0;
        string linea;
        while ((linea = lector.ReadLine()) != null)
        {
            string[] objetos = linea.Split(',');
            for (int x = 0; x < objetos.Length; x++)
            {
                if (int.TryParse(objetos[x], out int valor) && valor > 0)
                    resultado[new Vector2(x, y)] = valor;
            }
            y++;
        }
        return resultado;
    }

    /// <summary>
    /// Aplica una posición guardada directamente al personaje.
    /// Se llama desde <see cref="EscenaSeleccionada"/> al continuar una partida guardada.
    /// </summary>
    /// <param name="x">Coordenada X donde se colocará el personaje.</param>
    /// <param name="y">Coordenada Y donde se colocará el personaje.</param>
    public void AplicarPosicionGuardada(float x, float y)
    {
        personaje.position = new Vector2(x, y);
    }
}