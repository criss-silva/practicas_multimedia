using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace capybara;

/// <summary>
/// Escena de pausa del juego. Se apila sobre la escena de juego activa cuando
/// el jugador pausa la partida, mostrando un overlay semitransparente y dos
/// botones: reanudar la partida o volver al menú principal.
/// Al activarse detiene la música de fondo si estuviera sonando.
/// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
/// </summary>
internal class EscenaPausa : IScene
{
    /// <summary>Referencia al gestor de escenas para poder desapilar esta escena al reanudar.</summary>
    private SceneManager _sceneManager;

    /// <summary>Gestor de contenido usado para cargar las texturas de los botones.</summary>
    private ContentManager _content;

    /// <summary>Dispositivo gráfico necesario para crear la textura de overlay procedural.</summary>
    private GraphicsDevice _graphicsDevice;

    /// <summary>
    /// Textura de 1×1 píxel usado como overlay de oscurecimiento semitransparente.
    /// Se escala al tamaño completo de la pantalla en <see cref="Draw"/>.
    /// </summary>
    private Texture2D _fondoLiso;

    /// <summary>Textura del botón de reanudar partida.</summary>
    private Texture2D _texBotonReanudar;

    /// <summary>Textura del botón de volver al menú principal.</summary>
    private Texture2D _texBotonMenu;

    /// <summary>
    /// Rectángulo de posición y tamaño visual del botón de reanudar.
    /// Incluye toda la imagen decorativa del botón.
    /// </summary>
    private Rectangle _rectReanudar;

    /// <summary>
    /// Rectángulo de posición y tamaño visual del botón de menú.
    /// Incluye toda la imagen decorativa del botón.
    /// </summary>
    private Rectangle _rectMenu;

    /// <summary>
    /// Área de clic activa del botón de reanudar, más estrecha verticalmente
    /// que <see cref="_rectReanudar"/> para mayor precisión de interacción.
    /// </summary>
    private Rectangle colReanudar;

    /// <summary>
    /// Área de clic activa del botón de menú, más estrecha verticalmente
    /// que <see cref="_rectMenu"/> para mayor precisión de interacción.
    /// </summary>
    private Rectangle colMenu;
/// <summary>
/// Textura del botón de ajustes
/// </summary>
    private Rectangle colAjustes;

    //<summary>
    /// Textura del botón de ajustes 
    /// </summary>

    private Texture2D _texBotonAjustes;

    /// <summary>
    /// Rectángulo de posición y tamaño visual del botón de ajustes.
    /// </summary>
    private Rectangle _rectAjustes;

    /// <summary>
    /// Estado del ratón en el frame anterior. Se usa para detectar pulsaciones únicas
    /// (pressed + released) y evitar que mantener el botón pulsado genere múltiples acciones.
    /// </summary>
    private MouseState _estadoRatonAnterior;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="EscenaPausa"/>.
    /// Los rectángulos de los botones se calculan en <see cref="LoadContent"/>
    /// porque dependen del <see cref="GraphicsDevice.Viewport"/>.
    /// </summary>
    /// <param name="sceneManager">Gestor de escenas del juego.</param>
    /// <param name="content">Gestor de contenido para la carga de assets.</param>
    /// <param name="graphicsDevice">Dispositivo gráfico de MonoGame.</param>
    public EscenaPausa(SceneManager sceneManager, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _sceneManager = sceneManager;
        _content = content;
        _graphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Carga el overlay semitransparente y las texturas de los botones.
    /// Calcula los rectángulos visuales y las áreas de clic de cada botón,
    /// centrados horizontalmente con un margen izquierdo fijo de 290 píxeles.
    /// </summary>
    public void LoadContent()
    {
        // Overlay oscuro semitransparente para difuminar la escena de juego subyacente
        _fondoLiso = new Texture2D(_graphicsDevice, 1, 1);
        _fondoLiso.SetData(new[] { new Color(10, 10, 10, 180) });

        _texBotonReanudar = _content.Load<Texture2D>("boton_reanudar");
        _texBotonMenu = _content.Load<Texture2D>("boton_menu");
        _texBotonAjustes = _content.Load<Texture2D>("boton_ajustes");

        int anchoBoton = 700;
        int altoBoton = 350;
        int xCentrada = 290;

        _rectReanudar = new Rectangle(xCentrada, 25, anchoBoton, altoBoton);
        _rectMenu = new Rectangle(xCentrada, 225, anchoBoton, altoBoton);
        _rectAjustes = new Rectangle(xCentrada, 425, anchoBoton, altoBoton);

        // El área de clic es más estrecha verticalmente para coincidir con la zona real del botón
        int altoClic = 50;
        colReanudar = new Rectangle(xCentrada, _rectReanudar.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
        colMenu = new Rectangle(xCentrada, _rectMenu.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
        colAjustes = new Rectangle(xCentrada, _rectAjustes.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
    }

    /// <summary>
    /// Actualiza la lógica de la pausa cada frame. Detiene la música si estuviera
    /// sonando y detecta los clics sobre los botones para reanudar o ir al menú.
    /// </summary>
    /// <param name="gameTime">
    /// Información de tiempo del frame actual proporcionada por MonoGame.
    /// No se usa directamente pero es requerido por la interfaz <see cref="IScene"/>.
    /// </param>
    public void Update(GameTime gameTime)
    {
        MouseState estadoRatonActual = Mouse.GetState();

        // La música se detiene mientras el juego está pausado
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        Rectangle ratonRect = new Rectangle(estadoRatonActual.X, estadoRatonActual.Y, 1, 1);

        bool clicIzquierdo = estadoRatonActual.LeftButton == ButtonState.Pressed &&
                             _estadoRatonAnterior.LeftButton == ButtonState.Released;

        if (clicIzquierdo)
        {
            if (_rectReanudar.Intersects(ratonRect))
            {
                // Desapila esta escena de pausa, volviendo a la escena de juego subyacente
                _sceneManager.RemoveScene();
            }
            else if (_rectMenu.Intersects(ratonRect))
            {
                // Apila el menú principal sobre la pausa; la escena de juego queda enterrada
                MenuScene menu = new MenuScene(_sceneManager, _content, _graphicsDevice);
                menu.LoadContent();
                _sceneManager.AddScene(menu);
            }
            else if (_rectAjustes.Intersects(ratonRect))
            {
                SettingsScene ajustes = new SettingsScene(_sceneManager, _content, _graphicsDevice);
                ajustes.LoadContent();
                _sceneManager.AddScene(ajustes);
            }
        }

        _estadoRatonAnterior = estadoRatonActual;
    }

    /// <summary>
    /// Dibuja el overlay semitransparente sobre la pantalla completa y los dos
    /// botones con efecto hover (cambian a gris cuando el cursor pasa por encima).
    /// La escena de juego subyacente ya fue dibujada antes por el <see cref="SceneManager"/>.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle pantalla = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        spriteBatch.Draw(_fondoLiso, pantalla, Color.White);

        MouseState raton = Mouse.GetState();
        Rectangle puntaRaton = new Rectangle(raton.X, raton.Y, 1, 1);

        Color colorReanudar = colReanudar.Intersects(puntaRaton) ? Color.Gray : Color.White;
        Color colorMenu = colMenu.Intersects(puntaRaton) ? Color.Gray : Color.White;
        Color colorAjustes = colAjustes.Intersects(puntaRaton) ? Color.Gray : Color.White;
        spriteBatch.Draw(_texBotonReanudar, _rectReanudar, colorReanudar);
        spriteBatch.Draw(_texBotonMenu, _rectMenu, colorMenu);
        spriteBatch.Draw(_texBotonAjustes, _rectAjustes, colorAjustes);
    }
}