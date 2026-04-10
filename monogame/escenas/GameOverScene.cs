using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara;

/// <summary>
/// Escena que se muestra cuando el jugador agota todas sus vidas.
/// Presenta una animación de derrota y dos botones: reiniciar desde
/// el nivel 1 o volver al menú principal.
/// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
/// </summary>
public class GameOverScene : IScene
{
    /// <summary>Referencia al gestor de escenas para poder manipular la pila de navegación.</summary>
    private SceneManager _sceneManager;

    /// <summary>Gestor de contenido usado para cargar texturas.</summary>
    private ContentManager _content;

    /// <summary>Dispositivo gráfico necesario para crear texturas procedurales como <see cref="_pixel"/>.</summary>
    private GraphicsDevice _graphicsDevice;

    /// <summary>
    /// Textura de 1×1 píxel blanco usada para dibujar el overlay de color
    /// sobre toda la pantalla mediante escalado del rectángulo destino.
    /// </summary>
    private Texture2D _pixel;

    /// <summary>Textura del botón de reinicio.</summary>
    private Texture2D _texReiniciar;

    /// <summary>Textura del botón de regreso al menú.</summary>
    private Texture2D _texMenu;

    /// <summary>Rectángulo de posición y tamaño del botón de reinicio en pantalla.</summary>
    private Rectangle _rectReiniciar;
    private Rectangle colReiniciar;

    /// <summary>Rectángulo de posición y tamaño del botón de menú en pantalla.</summary>
    private Rectangle _rectMenu;
    private Rectangle colMenu;


    /// <summary>Color de tinte del botón de reinicio. Cambia a gris cuando el ratón pasa por encima.</summary>
    private Color _colorReiniciar = Color.White;

    /// <summary>Color de tinte del botón de menú. Cambia a gris cuando el ratón pasa por encima.</summary>
    private Color _colorMenu = Color.White;

    /// <summary>Color de tinte reservado para un posible botón de salida (actualmente sin uso activo).</summary>
    private Color _colorSalir = Color.White;

    /// <summary>Sprite sheet con los frames de la animación de derrota.</summary>
    private Texture2D _spriteSheet;

    /// <summary>Número de columnas (frames horizontales) del sprite sheet de derrota.</summary>
    private int _columnas = 3;

    /// <summary>Número de filas del sprite sheet de derrota.</summary>
    private int _filas = 1;

    /// <summary>Índice del frame actualmente visible de la animación.</summary>
    private int _frameActual = 0;

    /// <summary>Duración de cada frame de animación en segundos.</summary>
    private float _tiempoPorFrame = 0.15f;

    /// <summary>Acumulador de tiempo para el avance de la animación.</summary>
    private float _cronometro = 0f;

    /// <summary>Posición central en pantalla donde se dibuja la animación de derrota.</summary>
    private Vector2 _posAnimacion;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GameOverScene"/>.
    /// </summary>
    /// <param name="sm">Gestor de escenas del juego.</param>
    /// <param name="content">Gestor de contenido para la carga de assets.</param>
    /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
    public GameOverScene(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;
    }

    /// <summary>
    /// Carga todos los assets necesarios para la escena: textura de overlay,
    /// sprite sheet de animación y texturas de botones. Calcula los rectángulos
    /// de posición de los botones centrados horizontalmente en pantalla.
    /// </summary>
    public void LoadContent()
    {
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _spriteSheet = _content.Load<Texture2D>("animacion_perder");
        _posAnimacion = new Vector2(1280 / 2 + 50, 720 / 2 - 150);

        float escala = 0.5f;
        float ajusteAncho = 0.7f;
        float ajusteAlto = 0.6f;

        _texReiniciar = _content.Load<Texture2D>("boton_restart");
        _texMenu = _content.Load<Texture2D>("boton_menu");

        int anchoR = (int)(_texReiniciar.Width * escala * ajusteAncho);
        int altoR = (int)(_texReiniciar.Height * escala * ajusteAlto);
        _rectReiniciar = new Rectangle(1280 / 2 - (anchoR / 2), 200, anchoR, altoR);

        int anchoM = (int)(_texMenu.Width * escala * ajusteAncho);
        int altoM = (int)(_texMenu.Height * escala * ajusteAlto);
        _rectMenu = new Rectangle(1280 / 2 - (anchoM / 2), 350, anchoM, altoM);


         int altoClic = 80;
        colReiniciar = new Rectangle(1280 / 2 - (anchoR / 2), _rectReiniciar.Y + (altoR / 2) - (altoClic / 2), anchoR, altoClic);
        colMenu = new Rectangle(1280 / 2 - (anchoM / 2), _rectMenu.Y + (altoM / 2) - (altoClic / 2), anchoM, altoClic);
    }

    /// <summary>
    /// Actualiza la lógica de la escena cada frame: avanza la animación de derrota
    /// y gestiona el efecto hover y la pulsación de los botones de reinicio y menú.
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    public void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        Point mousePos = new Point(mouse.X, mouse.Y);

        _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cronometro >= _tiempoPorFrame)
        {
            _frameActual = (_frameActual + 1) % (_columnas * _filas);
            _cronometro = 0f;
        }

        if (_rectReiniciar.Contains(mousePos))
        {
            _colorReiniciar = Color.Gray;
            if (mouse.LeftButton == ButtonState.Pressed) ReiniciarNivel();
        }
        else _colorReiniciar = Color.White;

        if (_rectMenu.Contains(mousePos))
        {
            _colorMenu = Color.Gray;
            if (mouse.LeftButton == ButtonState.Pressed) IrAlMenu();
        }
        else _colorMenu = Color.White;
    }

    /// <summary>
    /// Reinicia la partida desde el nivel 1.
    /// Limpia la pila de escenas hasta dejar solo el <see cref="MenuScene"/>,
    /// resetea el <see cref="CollisionManager"/> y el <see cref="VidaManager"/>,
    /// y apila una nueva instancia de <see cref="GameScene"/> con el contenido cargado.
    /// </summary>
    private void ReiniciarNivel()
    {
        while (_sceneManager.sceneaActual() is not MenuScene)
        {
            _sceneManager.RemoveScene();
        }

        CollisionManager.Clear();
        VidaManager.Resetear();

        GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
        nivel1.LoadContent();
        _sceneManager.AddScene(nivel1);
    }

    /// <summary>
    /// Regresa al menú principal.
    /// Limpia la pila de escenas hasta dejar solo el <see cref="MenuScene"/>
    /// y resetea el <see cref="CollisionManager"/> y el <see cref="VidaManager"/>.
    /// </summary>
    private void IrAlMenu()
    {
        while (_sceneManager.sceneaActual() is not MenuScene)
            _sceneManager.RemoveScene();

        CollisionManager.Clear();
        VidaManager.Resetear();
    }

    /// <summary>
    /// Dibuja la escena de game over: overlay rojo semitransparente sobre toda la
    /// pantalla, los botones de acción con su tinte de hover y la animación de
    /// derrota recortada del sprite sheet y centrada en pantalla.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {


        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Red * 0.7f);
           
        Color colorReanudar = colReiniciar.Contains(Mouse.GetState().Position) ? Color.Gray : Color.White;
        Color colorMenu = colMenu.Contains(Mouse.GetState().Position) ? Color.Gray : Color.White;

        spriteBatch.Draw(_texReiniciar, _rectReiniciar, _colorReiniciar);
        spriteBatch.Draw(_texMenu, _rectMenu, _colorMenu);

        int w = _spriteSheet.Width / _columnas;
        int h = _spriteSheet.Height / _filas;
        Rectangle fuente = new Rectangle(
            (_frameActual % _columnas) * w,
            (_frameActual / _columnas) * h,
            w, h);

        spriteBatch.Draw(_spriteSheet, _posAnimacion, fuente, Color.White, 0f,
            new Vector2(w / 2, h / 2), 0.3f, SpriteEffects.None, 0f);
    }
}