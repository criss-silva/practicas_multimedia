using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara;

/// <summary>
/// Escena que se muestra cuando el jugador completa todos los niveles del juego.
/// Presenta un overlay dorado, una animación de victoria y un botón para
/// regresar al menú principal.
/// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
/// </summary>
public class WinScene : IScene
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

    /// <summary>Textura del botón de regreso al menú principal.</summary>
    private Texture2D _texturaBoton;

    /// <summary>Rectángulo de posición y tamaño del botón en pantalla.</summary>
    private Rectangle _rectBoton;

    /// <summary>Color de tinte del botón. Cambia a gris cuando el ratón pasa por encima.</summary>
    private Color _tinteBoton = Color.White;

    /// <summary>Sprite sheet con los frames de la animación de victoria.</summary>
    private Texture2D _spriteSheet;

    /// <summary>Número de columnas (frames horizontales) del sprite sheet de victoria.</summary>
    private int _columnas = 3;

    /// <summary>Número de filas del sprite sheet de victoria.</summary>
    private int _filas = 1;

    /// <summary>Número total de frames de la animación, calculado como <c>columnas × filas</c>.</summary>
    private int _totalFrames;

    /// <summary>Índice del frame actualmente visible de la animación.</summary>
    private int _frameActual = 0;

    /// <summary>Duración de cada frame de animación en segundos (≈ 15 fps con 0.15 s).</summary>
    private float _tiempoPorFrame = 0.15f;

    /// <summary>Acumulador de tiempo para el avance de la animación.</summary>
    private float _cronometro = 0f;

    /// <summary>Posición central en pantalla donde se dibuja la animación de victoria.</summary>
    private Vector2 _posAnimacion;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="WinScene"/>.
    /// </summary>
    /// <param name="sm">Gestor de escenas del juego.</param>
    /// <param name="content">Gestor de contenido para la carga de assets.</param>
    /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
    public WinScene(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;
    }

    /// <summary>
    /// Carga todos los assets necesarios para la escena: sprite sheet de animación,
    /// textura de overlay y textura del botón de menú. Calcula el rectángulo del
    /// botón centrado horizontalmente en pantalla.
    /// </summary>
    public void LoadContent()
    {
        _spriteSheet = _content.Load<Texture2D>("animacion_ganar");
        _totalFrames = _columnas * _filas;
        _posAnimacion = new Vector2(1280 / 2, 720 / 2 - 150);

        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _texturaBoton = _content.Load<Texture2D>("boton_menu");

        int anchoFinal = 600;
        int altoFinal = 300;
        int posX = (1280 / 2) - (anchoFinal / 2);
        int posY = 300;

        _rectBoton = new Rectangle(posX, posY, anchoFinal, altoFinal);
    }

    /// <summary>
    /// Actualiza la lógica de la escena cada frame: avanza la animación de victoria
    /// mediante un cronómetro y gestiona el efecto hover y la pulsación del botón
    /// de regreso al menú.
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    public void Update(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();
        Point mousePosition = new Point(mouseState.X, mouseState.Y);

        _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cronometro >= _tiempoPorFrame)
        {
            _frameActual++;
            if (_frameActual >= _totalFrames) _frameActual = 0;
            _cronometro = 0f;
        }

        if (_rectBoton.Contains(mousePosition))
        {
            _tinteBoton = Color.Gray;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                RegresarAlMenu();
            }
        }
        else
        {
            _tinteBoton = Color.White;
        }
    }

    /// <summary>
    /// Regresa al menú principal.
    /// Limpia la pila de escenas hasta dejar solo el <see cref="MenuScene"/>,
    /// resetea el <see cref="CollisionManager"/> para eliminar colisionadores
    /// residuales y resetea el <see cref="VidaManager"/> a su estado inicial.
    /// </summary>
    private void RegresarAlMenu()
    {
        while (_sceneManager.sceneaActual() is not MenuScene)
            _sceneManager.RemoveScene();

        CollisionManager.Clear();
        VidaManager.Resetear();
    }

    /// <summary>
    /// Dibuja la escena de victoria: overlay dorado semitransparente sobre toda la
    /// pantalla, el botón de menú con su tinte de hover y la animación de victoria
    /// recortada del sprite sheet, centrada en pantalla y escalada.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Gold * 0.8f);
        spriteBatch.Draw(_texturaBoton, _rectBoton, _tinteBoton);

        int anchoFrame = _spriteSheet.Width / _columnas;
        int altoFrame = _spriteSheet.Height / _filas;

        int columna = _frameActual % _columnas;
        int fila = _frameActual / _columnas;

        Rectangle fuente = new Rectangle(
            columna * anchoFrame,
            fila * altoFrame,
            anchoFrame,
            altoFrame
        );

        float escalaPequeña = 0.2f;
        Vector2 centroFrame = new Vector2(anchoFrame / 2, altoFrame / 2);

        spriteBatch.Draw(
            _spriteSheet,
            _posAnimacion,
            fuente,
            Color.White,
            0f,
            centroFrame,
            escalaPequeña,
            SpriteEffects.None,
            0f
        );
    }
}