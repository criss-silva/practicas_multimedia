using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara
{
    /// <summary>
    /// Escena de selección de modo de partida. Se muestra tras pulsar el botón
    /// de jugar en el <see cref="MenuScene"/> y ofrece al jugador dos opciones:
    /// iniciar una partida nueva desde el nivel 1 o continuar la partida guardada.
    /// El botón de continuar solo está activo si existe un archivo de guardado.
    /// Implementa <see cref="IScene"/> para integrarse con el <see cref="SceneManager"/>.
    /// </summary>
    public class EscenaSeleccionada : IScene
    {
        /// <summary>Referencia al gestor de escenas para apilar la escena de juego seleccionada.</summary>
        private SceneManager _sceneManager;

        /// <summary>Gestor de contenido para cargar las texturas de los botones.</summary>
        private ContentManager _content;

        /// <summary>Dispositivo gráfico necesario para crear la textura de píxel procedural.</summary>
        private GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Textura de 1×1 píxel blanco, reservada para posibles usos de debug o overlays.
        /// Se crea en <see cref="LoadContent"/>.
        /// </summary>
        private Texture2D _pixel;

        /// <summary>
        /// Referencia a la animación de fondo ya instanciada y compartida desde <see cref="MenuScene"/>.
        /// Se reutiliza para mantener la continuidad visual sin reiniciar la animación.
        /// </summary>
        private AnimacionFondo _animacionfondo;

        /// <summary>
        /// Rectángulo de posición y tamaño del botón de nueva partida en pantalla.
        /// También actúa como área de clic.
        /// </summary>
        private Rectangle _rectStart;

        /// <summary>
        /// Rectángulo de posición y tamaño del botón de continuar en pantalla.
        /// También actúa como área de clic.
        /// </summary>
        private Rectangle _rectContinue;

        /// <summary>Textura del botón de nueva partida.</summary>
        private Texture2D startTexture;

        /// <summary>Textura del botón de continuar partida.</summary>
        private Texture2D continueTexture;

        /// <summary>
        /// Color de tinte del botón de nueva partida.
        /// Cambia a <c>Color.LightGreen</c> al pasar el cursor por encima.
        /// </summary>
        private Color _colorStart = Color.Green;

        /// <summary>
        /// Color de tinte del botón de continuar.
        /// Cambia a <c>Color.LightBlue</c> al hacer hover si hay guardado,
        /// o permanece en gris si no existe guardado.
        /// </summary>
        private Color _colorContinue = Color.Gray;

        /// <summary>
        /// Estado del ratón en el frame anterior. Se usa para detectar pulsaciones únicas
        /// (pressed + released) y evitar que mantener el botón pulsado genere múltiples acciones.
        /// </summary>
        private MouseState _mouseAnterior;

        /// <summary>
        /// Indica si existe un archivo de guardado válido.
        /// Se evalúa cada frame para reflejar cambios en tiempo real.
        /// Controla si el botón de continuar está activo o desactivado visualmente.
        /// </summary>
        private bool _haySave;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EscenaSeleccionada"/>.
        /// Calcula los rectángulos de los botones y comprueba si existe un guardado.
        /// </summary>
        /// <param name="sm">Gestor de escenas del juego.</param>
        /// <param name="content">Gestor de contenido para la carga de assets.</param>
        /// <param name="gd">Dispositivo gráfico de MonoGame.</param>
        /// <param name="animacionfondo">
        /// Instancia de la animación de fondo compartida desde <see cref="MenuScene"/>.
        /// </param>
        public EscenaSeleccionada(SceneManager sm, ContentManager content, GraphicsDevice gd, AnimacionFondo animacionfondo)
        {
            _sceneManager = sm;
            _content = content;
            _graphicsDevice = gd;
            _animacionfondo = animacionfondo;

            _rectStart = new Rectangle(720, 210, 400, 300);
            _rectContinue = new Rectangle(160, 210, 400, 300);

            _haySave = SaveManager.ExisteSave();
            _colorContinue = _haySave ? Color.Blue : Color.Gray;
        }

        /// <summary>
        /// Carga las texturas de los botones de nueva partida y continuar.
        /// También crea la textura de píxel procedural por si se necesita en el futuro.
        /// </summary>
        public void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            startTexture = _content.Load<Texture2D>("boton_nuevo_juego");
            continueTexture = _content.Load<Texture2D>("boton_continuar");
        }

        /// <summary>
        /// Actualiza la lógica de la escena cada frame. Avanza la animación de fondo,
        /// detecta el hover y los clics sobre los botones, y gestiona las transiciones
        /// a una nueva partida o a la partida guardada.
        /// </summary>
        /// <param name="gameTime">
        /// Información de tiempo del frame actual proporcionada por MonoGame.
        /// </param>
        public void Update(GameTime gameTime)
        {
            // Se comprueba cada frame por si el save cambia en tiempo de ejecución
            _haySave = SaveManager.ExisteSave();

            _animacionfondo.Update(gameTime);
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = new Point(mouseActual.X, mouseActual.Y);

            // --- Botón Nueva Partida ---
            if (_rectStart.Contains(mousePos))
            {
                _colorStart = Color.LightGreen;
                if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                {
                    ModoJuego.EsSeleccionDeMundo = false;
                    SaveManager.BorrarSave();
                    CollisionManager.Clear();
                    InstruccionesManager.Reset(); // Permite mostrar instrucciones al empezar nueva partida
                    GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
                    nivel1.LoadContent();
                    _sceneManager.AddScene(nivel1);
                    return;
                }
            }
            else _colorStart = Color.Green;

            // --- Botón Continuar Partida ---
            if (_rectContinue.Contains(mousePos))
            {
                if (_haySave)
                {
                    _colorContinue = Color.LightBlue;
                    if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        ContinuarPartida();
                    }
                }
                else
                {
                    // Sin guardado el botón aparece desactivado visualmente
                    _colorContinue = Color.DimGray;
                }
            }
            else
            {
                _colorContinue = _haySave ? Color.Blue : Color.Gray;
            }

            _mouseAnterior = mouseActual;
        }

        /// <summary>
        /// Dibuja el fondo animado y los dos botones de selección sobre el <see cref="SpriteBatch"/> activo.
        /// </summary>
        /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _animacionfondo.Draw(spriteBatch, new Rectangle(0, 0, 1280, 720));
            spriteBatch.Draw(startTexture, _rectStart, Color.White);
            spriteBatch.Draw(continueTexture, _rectContinue, Color.White);
        }

        /// <summary>
        /// Carga la escena del nivel correspondiente al guardado existente,
        /// aplica la posición guardada del jugador y la apila en el gestor de escenas.
        /// Si no existe guardado no realiza ninguna acción.
        /// </summary>
        private void ContinuarPartida()
        {
            var save = SaveManager.Cargar();
            if (save == null) return;

            // Se selecciona la escena según el número de nivel guardado
            IScene nivel = save.Nivel switch
            {
                1 => new GameScene(_sceneManager, _content, _graphicsDevice),
                2 => new GameScene2(_sceneManager, _content, _graphicsDevice),
                3 => new GameScene3(_sceneManager, _content, _graphicsDevice),
                4 => new GameScene_M2(_sceneManager, _content, _graphicsDevice),
                5 => new GameScene2_M2(_sceneManager, _content, _graphicsDevice),
                6 => new GameScene3_M2(_sceneManager, _content, _graphicsDevice),
                _ => new GameScene(_sceneManager, _content, _graphicsDevice)
            };

            CollisionManager.Clear();
            nivel.LoadContent();

            // Se restaura la posición exacta del jugador en el nivel guardado
            if (nivel is GameScene gs) gs.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene2 gs2) gs2.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene3 gs3) gs3.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene_M2 gsm2) gsm2.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene2_M2 gs2m2) gs2m2.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene3_M2 gs3m3) gs3m3.AplicarPosicionGuardada(save.PosX, save.PosY);

            _sceneManager.AddScene(nivel);
        }
    }
}