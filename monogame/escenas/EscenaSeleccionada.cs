using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara
{
    /// <summary>
    /// Escena de selección de modo de juego. Aparece al pulsar Play en el menú
    /// principal y permite al jugador elegir entre iniciar una partida nueva
    /// o continuar la partida guardada localmente.
    /// </summary>
    public class EscenaSeleccionada : IScene
    {
        private SceneManager _sceneManager;
        private ContentManager _content;
        private GraphicsDevice _graphicsDevice;

        private Texture2D _botonStart;
        private Texture2D _botonContinue;
        private Texture2D _fondo;

        private Rectangle _rectStart;
        private Rectangle _rectContinue;
        private Rectangle _colStart;
        private Rectangle _colContinue;

        private Color _colorStart = Color.White;
        private Color _colorContinue = Color.White;

        private bool _haySave;
        private MouseState _mouseAnterior;

        public EscenaSeleccionada(SceneManager sm, ContentManager content, GraphicsDevice gd)
        {
            _sceneManager = sm;
            _content = content;
            _graphicsDevice = gd;

            // Ajusta estas coordenadas cuando tengas los botones
            int anchoBoton = 600;
            int altoBoton = 300;
            int altoClic = 100;
            int xCentrada = 330;

            _rectStart = new Rectangle(xCentrada, 220, anchoBoton, altoBoton);
            _rectContinue = new Rectangle(xCentrada, 375, anchoBoton, altoBoton);

            _colStart = new Rectangle(xCentrada, _rectStart.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
            _colContinue = new Rectangle(xCentrada, _rectContinue.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
        }

        public void LoadContent()
        {
            // Cambia los nombres por los de tus texturas cuando las tengas
            _botonStart = _content.Load<Texture2D>("boton_start");
            _botonContinue = _content.Load<Texture2D>("boton_continue");
            _fondo = _content.Load<Texture2D>("fondo");

            _haySave = SaveManager.ExisteSave();
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = new Point(mouseActual.X, mouseActual.Y);

            // Botón Start — nueva partida desde el nivel 1
            if (_colStart.Contains(mousePos))
            {
                _colorStart = Color.LightGray;
                if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                {
                    SaveManager.BorrarSave();
                    GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
                    nivel1.LoadContent();
                    _sceneManager.AddScene(nivel1);
                }
            }
            else _colorStart = Color.White;

            // Botón Continue — solo si hay save
            if (_haySave && _colContinue.Contains(mousePos))
            {
                _colorContinue = Color.LightGray;
                if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                {
                    ContinuarPartida();
                }
            }
            else _colorContinue = Color.White;

            _mouseAnterior = mouseActual;
        }

        private void ContinuarPartida()
        {
            var save = SaveManager.Cargar();
            if (save == null) return;

            IScene nivel = save.Nivel switch
            {
                1 => new GameScene(_sceneManager, _content, _graphicsDevice),
                2 => new GameScene2(_sceneManager, _content, _graphicsDevice),
                3 => new GameScene3(_sceneManager, _content, _graphicsDevice),
                _ => new GameScene(_sceneManager, _content, _graphicsDevice)
            };

            nivel.LoadContent();

            if (nivel is GameScene gs) gs.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene2 gs2) gs2.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene3 gs3) gs3.AplicarPosicionGuardada(save.PosX, save.PosY);

            _sceneManager.AddScene(nivel);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_fondo, new Rectangle(0, 0, 1280, 720), Color.White);

            spriteBatch.Draw(_botonStart, _rectStart, _colorStart);

            // El botón continue se dibuja en gris apagado si no hay save
            Color tinteContinue = !_haySave ? Color.DarkGray : _colorContinue;
            spriteBatch.Draw(_botonContinue, _rectContinue, tinteContinue);
        }
    }
}