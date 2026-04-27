using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace capybara;

/// <summary>
/// Clase principal del juego. Punto de entrada de la aplicación MonoGame.
/// Hereda de <see cref="Game"/> y es responsable de inicializar el motor gráfico,
/// cargar el contenido inicial, gestionar el ciclo de vida del juego (Update / Draw)
/// y coordinar el <see cref="SceneManager"/> con el <see cref="HUD"/>.
/// </summary>
public class Game1 : Game
{
    /// <summary>
    /// Gestor del dispositivo gráfico. Permite configurar resolución,
    /// modo de pantalla completa y otros parámetros de presentación.
    /// </summary>
    private GraphicsDeviceManager _graphics;

    /// <summary>
    /// Objeto principal de renderizado por lotes. Agrupa todas las llamadas
    /// de dibujo de un frame para minimizar las llamadas a la GPU.
    /// </summary>
    private SpriteBatch _spriteBatch;

    /// <summary>
    /// Gestor de escenas basado en pila. Controla qué escena está activa
    /// en cada momento y permite la transición entre ellas.
    /// </summary>
    private SceneManager sceneManager;

    /// <summary>
    /// Heads-Up Display del juego. Muestra información persistente como
    /// las vidas restantes del jugador. Solo se dibuja durante las escenas de juego.
    /// </summary>
    private HUD _hud;

    /// <summary>
    /// Guarda el estado del teclado del frame anterior. 
    /// Esencial para detectar cuándo una tecla acaba de ser pulsada (Single Press)
    /// y evitar que la transición de pausa parpadee a 60 frames por segundo.
    /// </summary>
    private KeyboardState _estadoTecladoAnterior;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Game1"/>.
    /// Configura la resolución de la ventana a 1280x720, hace visible el cursor
    /// del ratón e instancia el <see cref="SceneManager"/>.
    /// </summary>
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        sceneManager = new SceneManager();
    }

    /// <summary>
    /// Inicialización del motor MonoGame. Invoca la inicialización base
    /// antes de cualquier configuración adicional.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Carga todos los recursos persistentes del juego que existen durante
    /// toda la sesión, independientemente de la escena activa.
    /// Crea el <see cref="SpriteBatch"/>, inicializa el <see cref="HUD"/> y
    /// apila la escena inicial (<see cref="MenuScene"/>) en el <see cref="SceneManager"/>.
    /// </summary>
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _hud = new HUD();
        _hud.LoadContent(Content);

        MenuScene menu = new MenuScene(sceneManager, Content, GraphicsDevice);
        menu.LoadContent();
        sceneManager.AddScene(menu);
    }

    /// <summary>
    /// Ciclo de actualización lógica del juego. Se ejecuta una vez por frame.
    /// Delega la actualización a la escena que se encuentre en la cima de la pila
    /// del <see cref="SceneManager"/>. También gestiona la entrada al menú de pausa.
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    protected override void Update(GameTime gameTime)
    {
        KeyboardState estadoTecladoActual = Keyboard.GetState();
        IScene escenaActual = sceneManager.sceneaActual();

        // Detectamos si la tecla Escape ha sido presionada en este frame exacto
        if (estadoTecladoActual.IsKeyDown(Keys.Escape) && _estadoTecladoAnterior.IsKeyUp(Keys.Escape))
        {
            if (escenaActual is EscenaPausa) 
            {

                sceneManager.RemoveScene(); 
            }
            else if (escenaActual is GameScene_M2 || escenaActual is GameScene2_M2 || escenaActual is GameScene3_M2)
            {
                
                EscenaPausa pausa = new EscenaPausa(sceneManager, Content, GraphicsDevice); 
                pausa.LoadContent();
                sceneManager.AddScene(pausa);
            }
        }

       
        _estadoTecladoAnterior = estadoTecladoActual;

        
        sceneManager.sceneaActual()?.Update(gameTime);

        base.Update(gameTime);
    }

    /// <summary>
    /// Ciclo de renderizado del juego. Se ejecuta una vez por frame tras <see cref="Update"/>.
    /// Limpia el buffer con el color de fondo, dibuja la escena activa y, únicamente
    /// si la escena actual es una escena de juego (<see cref="GameScene_M2"/>,
    /// <see cref="GameScene2_M2"/> o <see cref="GameScene3_M2"/>), dibuja el <see cref="HUD"/>
    /// por encima. El HUD no se muestra en menús ni en pantallas de victoria o derrota.
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(111, 94, 132));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

    
        sceneManager.sceneaActual()?.Draw(_spriteBatch);

        IScene escenaActual = sceneManager.sceneaActual();
        if (escenaActual is GameScene|| escenaActual is GameScene2 || escenaActual is GameScene3||escenaActual is GameScene_M2 || escenaActual is GameScene2_M2 || escenaActual is GameScene3_M2)
        {
            _hud.Draw(_spriteBatch);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}