using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace capybara
{

    /// <summary>
    /// Escena del menú principal del juego. Es la primera escena que ve el jugador
    /// al iniciar la aplicación y sirve como punto de retorno tras completar una
    /// partida o perder todas las vidas.
    /// Muestra el fondo, el título del juego y los botones de jugar y ajustes.
    /// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
    /// </summary>
    public class MenuScene : IScene
    {
       /// <summary>
       /// fondo animado para la pantalla de inicio
       /// </summary>
        private AnimacionFondo _animacionFondo;   
        /// <summary>Textura de fondo que ocupa toda la pantalla.</summary>
        private Texture2D _fondo;

        /// <summary>Textura con el nombre o logotipo del juego.</summary>
        private Texture2D _nombreJuego;

        /// <summary>Textura del botón de jugar.</summary>
        private Texture2D boton_jugar;

        private Texture2D _botonSeleccionMundos;

        /// <summary>Textura del botón de ajustes.</summary>
        private Texture2D boton_ajustes;

        /// <summary>Referencia al gestor de escenas para poder apilar nuevas escenas.</summary>
        private SceneManager _sceneManager;

        /// <summary>Gestor de contenido usado para cargar texturas.</summary>
        private ContentManager _content;

        /// <summary>Dispositivo gráfico de MonoGame.</summary>
        private GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Rectángulo de destino donde se dibuja visualmente el botón de jugar.
        /// Incluye toda la imagen del botón con su decoración.
        /// </summary>
        private Rectangle rect_jugar;

        /// <summary>
        /// Rectángulo de destino donde se dibuja visualmente el botón de ajustes.
        /// Incluye toda la imagen del botón con su decoración.
        /// </summary>
        private Rectangle rect_ajustes;

        /// <summary>Rectángulo de destino donde se dibuja el nombre o logotipo del juego.</summary>
        private Rectangle rect_nombre;

        private Rectangle _rectSeleccionMundos;

        /// <summary>
        /// Área de clic activa del botón de jugar. Es más pequeña que <see cref="rect_jugar"/>
        /// y está centrada sobre la zona interactiva real del botón para mayor precisión.
        /// </summary>
        private Rectangle col_jugar;

        /// <summary>
        /// Área de clic activa del botón de ajustes. Es más pequeña que <see cref="rect_ajustes"/>
        /// y está centrada sobre la zona interactiva real del botón para mayor precisión.
        /// </summary>
        private Rectangle col_ajustes;

        private Rectangle _colSeleccionMundos;

        /// <summary>
        /// Estado del ratón en el frame anterior. Se usa para detectar pulsaciones únicas
        /// (pressed + released) y evitar que mantener el botón pulsado apile múltiples escenas.
        /// </summary>
        private MouseState _mouseAnterior;
        private Song musicaFondo;



        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MenuScene"/>.
        /// Calcula en el constructor todos los rectángulos de posición y las áreas
        /// de clic de los botones y del título, ya que no dependen de texturas cargadas.
        /// </summary>
        /// <param name="sm">Gestor de escenas del juego.</param>
        /// <param name="content">Gestor de contenido para la carga de assets.</param>
        /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
        public MenuScene(SceneManager sm, ContentManager content, GraphicsDevice gd)
        {
            _sceneManager = sm;
            _content = content;
            _graphicsDevice = gd;

            int anchoBoton = 600;
            int altoBoton = 300;
            int xCentrada = 330;
            int altoClic = 100;
            rect_jugar = new Rectangle(xCentrada, 220, anchoBoton, altoBoton);
            rect_ajustes = new Rectangle(xCentrada, 375, anchoBoton, altoBoton);

            // Botón Jugar — arriba
            rect_jugar = new Rectangle(xCentrada, 100, anchoBoton, altoBoton);
            col_jugar = new Rectangle(xCentrada, rect_jugar.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);

            // Botón Selección Mundos — en medio
            _rectSeleccionMundos = new Rectangle(xCentrada, 270, anchoBoton, altoBoton);
            _colSeleccionMundos = new Rectangle(xCentrada, _rectSeleccionMundos.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);

            // Botón Ajustes — abajo
            rect_ajustes = new Rectangle(xCentrada, 440, anchoBoton, altoBoton);
            col_ajustes = new Rectangle(xCentrada, rect_ajustes.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);

            int anchoNombre = 800;
            int altoNombre = 400;
            int xNombreCentrada = (1280 - anchoNombre) / 2;
            rect_nombre = new Rectangle(xNombreCentrada, -50, anchoNombre, altoNombre);

            _mouseAnterior = Mouse.GetState();
        }

        /// <summary>
        /// Carga todos los assets visuales del menú: fondo, título del juego
        /// y texturas de los botones de jugar y ajustes.
        /// </summary>
        public void LoadContent()
        {
            Texture2D sheetMenu = _content.Load<Texture2D>("fondo_burbuja");   
            _animacionFondo = new AnimacionFondo(sheetMenu, totalFrames: 16, fps: 7f);
            _nombreJuego = _content.Load<Texture2D>("nombre_juego");
            boton_jugar = _content.Load<Texture2D>("boton_jugar");
            boton_ajustes = _content.Load<Texture2D>("boton_ajustes");
            _botonSeleccionMundos = _content.Load<Texture2D>("boton_seleccion_mundos");
            musicaFondo = _content.Load<Song>("musica_menu");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(musicaFondo);
            MediaPlayer.Volume = 0.3f;
             
        }

        /// <summary>
        /// Actualiza la lógica del menú cada frame. Detecta si el jugador pulsa
        /// el botón de jugar mediante la comparación del estado actual y anterior
        /// del ratón para garantizar una única pulsación por click. Al confirmar
        /// la pulsación, crea y apila una nueva instancia de <see cref="GameScene_M2"/>.
        /// </summary>
        /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
       public void Update(GameTime gameTime)
        {
            
            _animacionFondo.Update(gameTime);
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = new Point(mouseActual.X, mouseActual.Y);
            if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != musicaFondo)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.3f;
                MediaPlayer.Play(musicaFondo);
            }
           

            bool clic = mouseActual.LeftButton == ButtonState.Pressed 
                     && _mouseAnterior.LeftButton == ButtonState.Released;

            if (clic)
            {
                if (col_jugar.Contains(mousePos))
                {
                    ModoJuego.EsSeleccionDeMundo = false;
                    EscenaSeleccionada seleccion = new EscenaSeleccionada(
                        _sceneManager, _content, _graphicsDevice, _animacionFondo);
                    seleccion.LoadContent();
                    _sceneManager.AddScene(seleccion);
                }
                else if (_colSeleccionMundos.Contains(mousePos))
                {
                    ModoJuego.EsSeleccionDeMundo = true;
                    CollisionManager.Clear();
                    EscenaSeleccionMundo seleccionMundo = new EscenaSeleccionMundo(
                        _sceneManager, _content, _graphicsDevice, _animacionFondo);
                    seleccionMundo.LoadContent();
                    _sceneManager.AddScene(seleccionMundo);
                }
            }

            _mouseAnterior = mouseActual;
        }

        /// <summary>
        /// Dibuja todos los elementos visuales del menú en el orden correcto:
        /// fondo, título del juego y los dos botones con efecto hover (cambian a gris
        /// cuando el cursor se encuentra sobre su área de clic).
        /// </summary>
        /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _animacionFondo.Draw(spriteBatch, new Rectangle(0, 0, 1280, 720));

            spriteBatch.Draw(_nombreJuego, rect_nombre, Color.White);

            Color colorJugar = col_jugar.Contains(Mouse.GetState().Position) ? Color.LightGray : Color.White;
            spriteBatch.Draw(boton_jugar, rect_jugar, colorJugar);

            Color colorSeleccion = _colSeleccionMundos.Contains(Mouse.GetState().Position) 
                ? Color.LightGray : Color.White;
            spriteBatch.Draw(_botonSeleccionMundos, _rectSeleccionMundos, colorSeleccion);

            Color colorSalir = col_ajustes.Contains(Mouse.GetState().Position) ? Color.LightGray : Color.White;
            spriteBatch.Draw(boton_ajustes, rect_ajustes, colorSalir);
        }
    }
}