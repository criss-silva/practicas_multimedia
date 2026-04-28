using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara
{
    public class EscenaSeleccionada : IScene
    {
        private SceneManager _sceneManager;
        private ContentManager _content;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _pixel;

        private AnimacionFondo _animacionfondo;

        private Rectangle _rectStart;
        private Rectangle _rectContinue;
        private Texture2D startTexture;
        private Texture2D continueTexture;
        private Color _colorStart = Color.Green;
        private Color _colorContinue = Color.Gray;

        private MouseState _mouseAnterior;
        private bool _haySave;


        public EscenaSeleccionada(SceneManager sm, ContentManager content, GraphicsDevice gd, AnimacionFondo animacionfondo)
        {
            _sceneManager = sm;
            _content = content;
            _graphicsDevice = gd;
            _animacionfondo = animacionfondo;

            // CAMBIO 1: rectángulos más grandes y mejor posicionados
            _rectStart = new Rectangle(720, 210, 400, 300);
            _rectContinue = new Rectangle(160, 210, 400, 300);

            _haySave = SaveManager.ExisteSave();
            _colorContinue = _haySave ? Color.Blue : Color.Gray;
        }

        public void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            startTexture = _content.Load<Texture2D>("boton_nuevo_juego");
            continueTexture = _content.Load<Texture2D>("boton_continuar");
        }

        public void Update(GameTime gameTime)
        {
            // CAMBIO 2: actualizar _haySave cada frame
            _haySave = SaveManager.ExisteSave();

            _animacionfondo.Update(gameTime);
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = new Point(mouseActual.X, mouseActual.Y);

            // Botón Start
            if (_rectStart.Contains(mousePos))
            {
                _colorStart = Color.LightGreen;
                if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                {
                    ModoJuego.EsSeleccionDeMundo = false;
                    SaveManager.BorrarSave();
                    CollisionManager.Clear(); // CAMBIO 3: limpiar colisionadores residuales
                    GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
                    nivel1.LoadContent();
                    _sceneManager.AddScene(nivel1);
                    return;
                }
            }
            else _colorStart = Color.Green;
           
            // Botón Continue
            if (_rectContinue.Contains(mousePos))
            {
                if (_haySave)
                {
                    _colorContinue = Color.LightBlue;
                    // CAMBIO 4: llamar a ContinuarPartida en vez del código duplicado
                    if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        ContinuarPartida();
                    }
                }
                else
                {
                    _colorContinue = Color.DimGray;
                }
            }
            else
            {
                _colorContinue = _haySave ? Color.Blue : Color.Gray;
            }

            _mouseAnterior = mouseActual;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animacionfondo.Draw(spriteBatch, new Rectangle(0, 0, 1280, 720));
            spriteBatch.Draw(startTexture, _rectStart, Color.White);
            spriteBatch.Draw(continueTexture, _rectContinue, Color.White);
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
                
                4 => new GameScene_M2(_sceneManager, _content, _graphicsDevice),
                5 => new GameScene2_M2(_sceneManager, _content, _graphicsDevice),
                6 => new GameScene3_M2(_sceneManager, _content, _graphicsDevice),

                _ => new GameScene(_sceneManager, _content, _graphicsDevice)
            };

            CollisionManager.Clear(); // CAMBIO 5: limpiar antes de cargar
            nivel.LoadContent();

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