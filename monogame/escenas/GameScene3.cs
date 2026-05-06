using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace capybara;

/// <summary>
/// Tercera y última escena de juego jugable. Carga y gestiona el nivel 3 a partir
/// de su archivo CSV, controla al personaje, gestiona un enemigo volador activo
/// y detecta colisiones con tiles especiales. Al alcanzar el tile de victoria
/// lanza la pantalla de <see cref="WinScene"/>; al agotar las vidas, la de
/// <see cref="GameOverScene"/>.
/// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
/// </summary>
public class GameScene3 : IScene
{
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
    /// Textura de 1×1 píxel blanco usada para dibujar rectángulos de depuración
    /// (bounding boxes) mediante escalado del rectángulo destino.
    /// </summary>
    private Texture2D pixel;

    /// <summary>
    /// Enemigo volador presente en el nivel 3. Persigue al jugador una vez
    /// que este entra en su radio de detección y le inflige daño al contacto.
    /// </summary>
    private Enemigo enemigo;

    /// <summary>Referencia al gestor de escenas para poder apilar nuevas escenas.</summary>
    private SceneManager _sceneManager;

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

    /// </summary>Variable dedicada a la textura del fondo
    private Texture2D _fondo;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GameScene3"/>.
    /// Se suscribe a los eventos de <see cref="VidaManager"/> para reaccionar
    /// ante la pérdida de vida (respawn) y el game over.
    /// </summary>
    /// <param name="sm">Gestor de escenas del juego.</param>
    /// <param name="content">Gestor de contenido para la carga de assets.</param>
    /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
    /// 
    /// 
    private Song musica_nivel;
    public GameScene3(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        this.Content = content;
        this._graphicsDevice = gd;

        VidaManager.OnPerderVida += Respawn;
        VidaManager.OnGameOver += GameOver;
    }

    /// <summary>
    /// Carga todos los recursos del nivel 3: tilemap desde CSV, fondo del nivel, tilesheet,
    /// sprites del personaje, sus animaciones y el enemigo volador. Limpia el
    /// <see cref="CollisionManager"/> al inicio para evitar colisionadores residuales
    /// de escenas anteriores y registra los colisionadores del personaje, el enemigo
    /// y todos los tiles sólidos.
    /// <para>
    /// Tiles excluidos de la generación de colisionadores:
    /// <list type="bullet">
    ///   <item><description>18 — tile decorativo sin colisión física.</description></item>
    ///   <item><description>99 — reservado; no se usa en este nivel.</description></item>
    ///   <item><description>50 — tile de muerte (trigger de pérdida de vida).</description></item>
    ///   <item><description>100 — tile de victoria (trigger de transición a <see cref="WinScene"/>).</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public void LoadContent()
    {
        CollisionManager.Clear();

        tilemap = CargarMapa("Content/nivel3.csv");
        _fondo = Content.Load<Texture2D>("fondo_nivel1");
        textureAtlas = Content.Load<Texture2D>("tilesheet");
        Texture2D texturecapibara = Content.Load<Texture2D>("personaje_basico");
        Texture2D texAnimacion = Content.Load<Texture2D>("animacion_burbuja");
        Texture2D texSalida = Content.Load<Texture2D>("animacion_romper_burbuja");
        SoundEffect sonidoSalto = Content.Load<SoundEffect>("sonido_salto");
        SoundEffect sonidoBurbuja = Content.Load<SoundEffect>("sonido_entrar_burbuja");
        SoundEffect sonidoFueraburbuja = Content.Load<SoundEffect>("sonido_salir_burbuja");
        personaje = new MovedSprite(texturecapibara, new Vector2(100, 90), escala, velocidad, texAnimacion, texSalida,sonidoSalto, sonidoBurbuja, sonidoFueraburbuja);
        sprites = new List<Sprite> { personaje };

        enemigo = new Enemigo(Content.Load<Texture2D>("enemigo1"), new Vector2(800, 250), 0.1f, 2);
        enemigo.Collider.Owner = "enemy";
        enemigo.SetDebugMode(false);

        CollisionManager.AddCollider(personaje.Collider);
        CollisionManager.AddCollider(enemigo.Collider);

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

        int tileSize = 60;
        foreach (var item in tilemap)
        {
            if (item.Value != 18 && item.Value != 99 && item.Value != 50 && item.Value != 100)
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
    /// personaje con su posición visual, actualiza el <see cref="CollisionManager"/>,
    /// procesa la entrada del jugador, actualiza el enemigo y comprueba colisiones
    /// con tiles especiales.
    /// <para>
    /// Tiles especiales gestionados:
    /// <list type="bullet">
    ///   <item><description>50 — al tocarlo se invoca <see cref="VidaManager.PerderVida"/> y se resetea la posición del enemigo.</description></item>
    ///   <item><description>100 — al tocarlo se desuscriben los eventos, se limpia el <see cref="CollisionManager"/> y se carga <see cref="WinScene"/>.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    public void Update(GameTime gameTime)
    {
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
        enemigo.Update(gameTime, personaje, tilemap);

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
                    VidaManager.PerderVida();
                    enemigo.ResetearPosicion();
                    break;
                }
            }

            if (item.Value == 100)
            {
                Rectangle tileWin = new Rectangle(
                    (int)item.Key.X * tileSize,
                    (int)item.Key.Y * tileSize,
                    tileSize, tileSize);

                if (personaje.Rect.Intersects(tileWin))
                {
                    VidaManager.OnPerderVida -= Respawn;
                    VidaManager.OnGameOver -= GameOver;
                    CollisionManager.Clear();

                    if (ModoJuego.EsSeleccionDeMundo)
                    {
                        // MODO SELECCIÓN: pantalla de fin de mundo
                        var finMundo = new EscenaFinMundo(_sceneManager, Content, _graphicsDevice);
                        finMundo.LoadContent();
                        _sceneManager.AddScene(finMundo);
                    }
                    else
                    {
                        // MODO SECUENCIAL: pantalla de carga antes del Mundo 2
                        var carga = new EscenaCarga(_sceneManager, Content, _graphicsDevice);
                        carga.LoadContent();
                        _sceneManager.AddScene(carga);
                    }
                    return;
                }
            }
        }

        teclaanterior = tecladoActual;
    }

    /// <summary>
    /// Dibuja todos los elementos visuales del nivel en el orden correcto:
    /// Primero el fondo del nivel, seguido los tiles del tilemap (omitiendo los tiles especiales sin textura),
    /// luego el enemigo, después el personaje con su animación activa (normal,
    /// burbuja o salida de burbuja) y finalmente el bounding box de depuración
    /// en rojo semitransparente.
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

        enemigo.Draw(spriteBatch);

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

        spriteBatch.Draw(pixel, personaje.Rect, Color.Red * 0.5f);
    }

    /// <summary>
    /// Lee un archivo CSV de tilemap y lo convierte en un diccionario de tiles.
    /// Cada fila del CSV corresponde a una fila de tiles y cada valor separado
    /// por coma a una columna. Solo se almacenan los tiles con valor mayor que 0.
    /// </summary>
    /// <param name="ruta">Ruta relativa al archivo CSV del nivel (p. ej. "Content/nivel3.csv").</param>
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
    /// Callback suscrito a <see cref="VidaManager.OnPerderVida"/>.
    /// Reposiciona al personaje en el punto de spawn inicial del nivel,
    /// anula su velocidad para evitar inercia residual y resetea la posición
    /// del enemigo a su punto de spawn original.
    /// </summary>
    private void Respawn()
    {
        personaje.position = new Vector2(100, 90);
        personaje.velocity = Vector2.Zero;
        enemigo.ResetearPosicion();
        enemigo.ResetearDeteccion();
    }

    /// <summary>
    /// Callback suscrito a <see cref="VidaManager.OnGameOver"/>.
    /// Se desuscribe de ambos eventos del <see cref="VidaManager"/> para evitar
    /// referencias colgadas, limpia el <see cref="CollisionManager"/> y apila
    /// la escena <see cref="GameOverScene"/> en el <see cref="SceneManager"/>.
    /// </summary>
    private void GameOver()
    {
    VidaManager.OnPerderVida -= Respawn;
    VidaManager.OnGameOver -= GameOver;

    SaveManager.BorrarSave(); 

    CollisionManager.Clear();
    GameOverScene gameOver = new GameOverScene(_sceneManager, Content, _graphicsDevice);
    gameOver.LoadContent();
    _sceneManager.AddScene(gameOver);
    }
        /// <summary>
    /// Aplica la posición guardada al personaje tras cargar el nivel.
    /// Se llama desde EscenaSeleccionada al continuar una partida.
    /// </summary>
    public void AplicarPosicionGuardada(float x, float y)
    {
        personaje.position = new Vector2(x, y);
        enemigo.ResetearPosicion();
    }
}