using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace capybara;

/// <summary>
/// Primera escena de juego del Mundo 2. Versión del nivel 1 con la estética
/// del segundo mundo (tilesheet y fondo propios). Carga el mismo CSV que
/// <see cref="GameScene"/> pero con assets visuales distintos, y al completarlo
/// avanza a <see cref="GameScene2_M2"/> en lugar de <see cref="GameScene2"/>.
/// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
/// </summary>
public class GameScene_M2 : IScene
{
    /// <summary>
    /// Diccionario que representa el tilemap del nivel. La clave es la posición
    /// del tile en coordenadas de cuadrícula (columna, fila) y el valor es el
    /// identificador numérico del tile en el tilesheet.
    /// </summary>
    private Dictionary<Vector2, int> tilemap;

    /// <summary>
    /// Referencia al checkpoint activo del nivel. Es <c>null</c> si el jugador
    /// aún no ha activado ningún checkpoint. Cuando está activo, se actualiza
    /// cada frame y sirve como punto de respawn.
    /// </summary>
    Checkpoint _checkpoint = null;

    /// <summary>
    /// Lista de rectángulos fuente que mapean cada identificador de tile
    /// a su posición correspondiente dentro del <see cref="textureAtlas"/>.
    /// El índice de la lista equivale al valor del tile menos 1.
    /// </summary>
    private List<Rectangle> texturas;

    /// <summary>Textura atlas (tilesheet del Mundo 2) que contiene todos los sprites de tiles del nivel.</summary>
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
    /// Controla si se debe mostrar el panel de instrucciones.
    /// Se inicializa consultando <see cref="InstruccionesManager"/> para que
    /// solo sea <c>true</c> en la primera partida nueva.
    /// </summary>
    private bool _mostrarInstrucciones = !InstruccionesManager.YaMostradas;

    /// <summary>Textura del panel de instrucciones mostrado al inicio del nivel.</summary>
    private Texture2D _texturaPanel;

    /// <summary>
    /// Rectángulo de destino del panel de instrucciones, centrado en pantalla.
    /// Se calcula en el constructor.
    /// </summary>
    private Rectangle _rectPanel;

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

    /// <summary>
    /// Objeto de tipo <see cref="objetos"/> que representa la columna interactiva
    /// del nivel. Gestiona la animación de la columna y la detección del jugador
    /// para guardar el progreso.
    /// </summary>
    private objetos columna;

    /// <summary>Textura de fondo específica del Mundo 2.</summary>
    private Texture2D _fondo;

    /// <summary>Dispositivo gráfico necesario para crear texturas procedurales.</summary>
    private GraphicsDevice _graphicsDevice;

    /// <summary>
    /// Coordenada X del punto de spawn o checkpoint guardado.
    /// Es <c>null</c> si se trata de una partida nueva sin posición guardada.
    /// </summary>
    private float? _spawnX;

    /// <summary>
    /// Coordenada Y del punto de spawn o checkpoint guardado.
    /// Es <c>null</c> si se trata de una partida nueva sin posición guardada.
    /// </summary>
    private float? _spawnY;

    /// <summary>
    /// Canción de fondo que suena durante los niveles de juego.
    /// Se reproduce en bucle desde <see cref="LoadContent"/>.
    /// </summary>
    private Song musica_nivel;




    /// <summary>
    /// Texto para las instrucciones
    /// </summary> 

    SpriteFont _fuenteInstrucciones;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GameScene_M2"/>.
    /// Guarda las referencias necesarias, suscribe los callbacks a los eventos
    /// del <see cref="VidaManager"/> y calcula el rectángulo del panel de instrucciones.
    /// </summary>
    /// <param name="sm">Gestor de escenas del juego.</param>
    /// <param name="content">Gestor de contenido para la carga de assets.</param>
    /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
    /// <param name="posX">
    /// Coordenada X de spawn opcional. Si se proporciona junto con <paramref name="posY"/>,
    /// el personaje aparecerá en esa posición en lugar de la posición inicial por defecto.
    /// </param>
    /// <param name="posY">
    /// Coordenada Y de spawn opcional. Si se proporciona junto con <paramref name="posX"/>,
    /// el personaje aparecerá en esa posición en lugar de la posición inicial por defecto.
    /// </param>
    public GameScene_M2(SceneManager sm, ContentManager content, GraphicsDevice gd, float? posX = null, float? posY = null)
    {
        _sceneManager = sm;
        this.Content = content;
        this._graphicsDevice = gd;

        _spawnX = posX;
        _spawnY = posY;

        VidaManager.OnPerderVida += Respawn;
        VidaManager.OnGameOver += GameOver;

        int anchoPanel = 400;
        int altoPanel = 300;
        _rectPanel = new Rectangle((1280 - anchoPanel) / 2, (720 - altoPanel) / 2, anchoPanel, altoPanel);
    }

    /// <summary>
    /// Carga todos los recursos del nivel 1 del Mundo 2: tilemap desde CSV, fondo y tilesheet
    /// propios del Mundo 2, sprites del personaje y sus animaciones. Registra los
    /// colisionadores del personaje y de todos los tiles sólidos en el <see cref="CollisionManager"/>.
    /// <para>
    /// Tiles excluidos de la generación de colisionadores:
    /// <list type="bullet">
    ///   <item><description>18 — tile decorativo sin colisión física.</description></item>
    ///   <item><description>50 — tile de muerte (trigger de pérdida de vida).</description></item>
    ///   <item><description>99 — tile de meta (trigger de transición al siguiente nivel).</description></item>
    ///   <item><description>200 — tile especial reservado sin colisión.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public void LoadContent()
    {
        tilemap = CargarMapa("Content/nivel1.csv");
        _fondo = Content.Load<Texture2D>("fondo_mundo_2");           // Fondo visual del Mundo 2
        textureAtlas = Content.Load<Texture2D>("tilesheet_mundo2");  // Tilesheet visual del Mundo 2
        Texture2D texturecapibara = Content.Load<Texture2D>("personaje_basico");
        Texture2D texAnimacion = Content.Load<Texture2D>("animacion_burbuja");
        Texture2D texSalida = Content.Load<Texture2D>("animacion_romper_burbuja");
        _texturaPanel = Content.Load<Texture2D>("instrucciones_");

        // Si hay posición guardada (checkpoint o continuar partida), se usa; si no, posición inicial
        Vector2 posicionInicial;
        if (_spawnX.HasValue && _spawnY.HasValue)
            posicionInicial = new Vector2(_spawnX.Value, _spawnY.Value);
        else
            posicionInicial = new Vector2(100, 90);

        SoundEffect sonidoSalto = Content.Load<SoundEffect>("sonido_salto");
        SoundEffect sonidoBurbuja = Content.Load<SoundEffect>("sonido_entrar_burbuja");
        SoundEffect sonidoFueraburbuja = Content.Load<SoundEffect>("sonido_salir_burbuja");

        personaje = new MovedSprite(texturecapibara, posicionInicial, escala, velocidad,
            texAnimacion, texSalida, sonidoSalto, sonidoBurbuja, sonidoFueraburbuja);
        sprites = new List<Sprite> { personaje };

        CollisionManager.AddCollider(personaje.Collider);
        personaje.Collider.Owner = "player";

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
            if (item.Value != 18 && item.Value != 99 && item.Value != 50 && item.Value != 200)
            {
                BoxCollider bloque = new BoxCollider(
                    new Vector2(item.Key.X * tileSize, item.Key.Y * tileSize),
                    tileSize, tileSize);
                CollisionManager.AddCollider(bloque);
            }
        }

        columna = new objetos(Content.Load<Texture2D>("columna_spritesheet"), new Vector2(210, 185), 2.0f, 12, nivel: 4);

        Texture2D texCaminar = Content.Load<Texture2D>("movimiento_capibara");
        personaje.TexCaminar = texCaminar;
        personaje.TotalFramesCaminar = 4;

        pixel = new Texture2D(_graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        musica_nivel = Content.Load<Song>("musica_niveles");

        try { _fuenteInstrucciones = Content.Load<SpriteFont>("fuente"); }
            catch { _fuenteInstrucciones = null; }
    }

    /// <summary>
    /// Actualiza la lógica del nivel cada frame. Si las instrucciones están visibles,
    /// solo procesa la entrada para cerrarlas y bloquea el resto del juego.
    /// En caso contrario: sincroniza el colisionador, actualiza el <see cref="CollisionManager"/>,
    /// procesa el movimiento, actualiza la columna y comprueba tiles especiales.
    /// <para>
    /// Tiles especiales gestionados:
    /// <list type="bullet">
    ///   <item><description>50 — al tocarlo se invoca <see cref="VidaManager.PerderVida"/>.</description></item>
    ///   <item><description>99 — al tocarlo se limpia el <see cref="CollisionManager"/> y se carga <see cref="GameScene2_M2"/>.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    public void Update(GameTime gameTime)
    {
        _checkpoint?.Update(gameTime, personaje);
        KeyboardState tecladoActual = Keyboard.GetState();

        if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != musica_nivel)
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(musica_nivel);
        }

        if (_mostrarInstrucciones)
        {
            if (tecladoActual.IsKeyDown(Keys.Enter) || tecladoActual.IsKeyDown(Keys.Space))
            {
                _mostrarInstrucciones = false;
                InstruccionesManager.Marcar();
            }
            teclaanterior = tecladoActual;
            return;
        }

        Rectangle playerRect = personaje.Rect;
        personaje.Collider.Position = new Vector2(playerRect.X, playerRect.Y);
        personaje.Collider.Width = playerRect.Width;
        personaje.Collider.Height = playerRect.Height;

        CollisionManager.Update();
        personaje.Update(tecladoActual, teclaanterior, gravedad, fuerza, gameTime);
        columna.Update(gameTime, personaje, tilemap, guardarProgreso: !ModoJuego.EsSeleccionDeMundo);

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
                    VidaManager.PerderVida();
                    break;
                }
            }

            if (item.Value == 99)
            {
                Rectangle tileMeta = new Rectangle(
                    (int)item.Key.X * tileSize,
                    (int)item.Key.Y * tileSize,
                    tileSize, tileSize);

                if (personaje.Rect.Intersects(tileMeta))
                {
                    CollisionManager.Clear();
                    // En el Mundo 2 el nivel siguiente es GameScene2_M2
                    GameScene2_M2 nivel2 = new GameScene2_M2(_sceneManager, Content, _graphicsDevice);
                    nivel2.LoadContent();
                    _sceneManager.AddScene(nivel2);
                    return;
                }
            }
        }

        teclaanterior = tecladoActual;
    }

    /// <summary>
    /// Dibuja todos los elementos visuales del nivel en el orden correcto:
    /// fondo del Mundo 2, tiles, áreas debug de columna y checkpoint, columna animada,
    /// personaje con su animación activa, bounding box de depuración y panel de instrucciones.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = 60;
        spriteBatch.Draw(_fondo, new Rectangle(0, 0, 1280, 720), Color.White);

        foreach (var item in tilemap)
        {
            if (item.Value == 99 || item.Value == 50 || item.Value == 18 || item.Value == 200) continue;
            Rectangle dest = new((int)item.Key.X * tileSize, (int)item.Key.Y * tileSize, tileSize, tileSize);
            spriteBatch.Draw(textureAtlas, dest, texturas[item.Value - 1], Color.White);
        }

   

        if (_checkpoint != null)
        {
            spriteBatch.Draw(pixel, new Rectangle(
                (int)_checkpoint.Rect.X,
                (int)_checkpoint.Rect.Y,
                _checkpoint.Rect.Width,
                _checkpoint.Rect.Height),
                Color.Blue * 0.4f);
        }

        columna.Draw(spriteBatch);

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
        else if (personaje.EstaCaminando && personaje.TexCaminar != null)
        {
            int anchoFrame = personaje.TexCaminar.Width / personaje.TotalFramesCaminar;
            int altoFrame = personaje.TexCaminar.Height;
            Rectangle fuente = new Rectangle(personaje.FrameActualCaminar * anchoFrame, 0, anchoFrame, altoFrame);
            Vector2 origen = new Vector2(anchoFrame / 2f, altoFrame / 2f);

            spriteBatch.Draw(personaje.TexCaminar, personaje.position, fuente, Color.White, 0f, origen, escala, personaje.efecto, 0f);
        }
        else
        {
            spriteBatch.Draw(personaje.texture, personaje.position, null, Color.White, 0f,
                new Vector2(personaje.texture.Width / 2f, personaje.texture.Height / 2f),
                escala, personaje.efecto, 0f);
        }

        

        if (_mostrarInstrucciones)
        {
            spriteBatch.Draw(pixel, new Rectangle(0, 0, 1280, 720), Color.Black * 0.6f);
            if (_texturaPanel != null)
                spriteBatch.Draw(_texturaPanel, _rectPanel, Color.White);
                DibujarTexto(spriteBatch, "Presiona Enter o Espacio para continuar", 1280, 720);
        }
    }

    /// <summary>
    /// Lee un archivo CSV de tilemap y lo convierte en un diccionario de tiles.
    /// Cada fila del CSV corresponde a una fila de tiles y cada valor separado
    /// por coma a una columna. Solo se almacenan los tiles con valor mayor que 0.
    /// </summary>
    /// <param name="ruta">Ruta relativa al archivo CSV del nivel (p. ej. <c>"Content/nivel1.csv"</c>).</param>
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
    /// Reposiciona al personaje en el checkpoint guardado o en el spawn inicial
    /// y anula su velocidad para evitar inercia residual.
    /// </summary>
    private void Respawn()
    {
        personaje.position = (_spawnX.HasValue && _spawnY.HasValue)
            ? new Vector2(_spawnX.Value, _spawnY.Value)
            : new Vector2(100, 90);
        personaje.velocity = Vector2.Zero;
    }

    /// <summary>
    /// Callback suscrito a <see cref="VidaManager.OnGameOver"/>.
    /// Se desuscribe de ambos eventos del <see cref="VidaManager"/> para evitar
    /// referencias colgadas, borra el guardado, limpia el <see cref="CollisionManager"/>
    /// y apila la escena <see cref="GameOverScene"/> en el <see cref="SceneManager"/>.
    /// </summary>
    private void GameOver()
    {
        VidaManager.OnPerderVida -= Respawn;
        VidaManager.OnGameOver -= GameOver;

        SaveManager.BorrarSave();

        CollisionManager.Clear();
        GameOverScene gameOver = new GameOverScene(_sceneManager, Content, _graphicsDevice, this.GetType());
        gameOver.LoadContent();
        _sceneManager.AddScene(gameOver);
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



    private void DibujarTexto(SpriteBatch sb, string texto, int w, int h)
        {
            if (_fuenteInstrucciones != null)
            {
                Vector2 tamTexto = _fuenteInstrucciones.MeasureString(texto);
                Vector2 posTexto = new Vector2((w - tamTexto.X) / 2f, h - tamTexto.Y - 30f);
                // Sombra
                sb.DrawString(_fuenteInstrucciones, texto, posTexto + new Vector2(2, 2), Color.Black * 0.6f);
                sb.DrawString(_fuenteInstrucciones, texto, posTexto, Color.White);
            }
           
        }
}