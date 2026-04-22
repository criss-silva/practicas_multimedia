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
            

            // Start — derecha
            _rectStart = new Rectangle(850, 260, 200, 200);
            // Continue — izquierda
            _rectContinue = new Rectangle(230, 260, 200, 200);

            _haySave = SaveManager.ExisteSave();

            _colorContinue = _haySave ? Color.Blue : Color.Gray;
        }

        public void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            _animacionfondo.Update(gameTime);
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = new Point(mouseActual.X, mouseActual.Y);

            // Hover y click en Start (Esto se queda igual)
            if (_rectStart.Contains(mousePos))
            {
                _colorStart = Color.LightGreen;
                if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                {
                    SaveManager.BorrarSave();
                    GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
                    nivel1.LoadContent();
                    _sceneManager.AddScene(nivel1);
                }
            }
            else _colorStart = Color.Green;

            // LÓGICA ACTUALIZADA: Hover y click en Continue
            if (_rectContinue.Contains(mousePos))
            {
                if (_haySave)
                {
                    // Hover cuando el botón está activo
                    _colorContinue = Color.LightBlue; 

                    // Clic en Continue
                    if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        SaveManager.Historial datos = SaveManager.Cargar();
                        
                        if (datos != null)
                        {
                            // AQUÍ CARGAS LA ESCENA
                            GameScene nivelCargado = new GameScene(_sceneManager, _content, _graphicsDevice);
                            
                            // Nota: Necesitarás un método en GameScene para pasarle los datos cargados.
                            // Por ejemplo: nivelCargado.CargarDesdeSave(datos.Nivel, datos.PosX, datos.PosY);
                            
                            nivelCargado.LoadContent();
                            _sceneManager.AddScene(nivelCargado);
                        }
                    }
                }
                else
                {
                    // Hover cuando el botón está inactivo (sin partida)
                    _colorContinue = Color.DimGray;
                }
            }
            else
            {
                // Estado normal del botón fuera del hover
                _colorContinue = _haySave ? Color.Blue : Color.Gray;
            }

            _mouseAnterior = mouseActual;
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            // Fondo de la pantalla
            _animacionfondo.Draw(spriteBatch, new Rectangle(0, 0, 1280, 720));
            // Cuadrado Start (derecha) — verde
            spriteBatch.Draw(_pixel, _rectStart, _colorStart);

            // Cuadrado Continue (izquierda) — gris apagado
            spriteBatch.Draw(_pixel, _rectContinue, _colorContinue);
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

            nivel.LoadContent(); // primero carga todo

            // luego sobreescribe la posición — este orden es correcto
            if (nivel is GameScene gs) gs.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene2 gs2) gs2.AplicarPosicionGuardada(save.PosX, save.PosY);
            else if (nivel is GameScene3 gs3) gs3.AplicarPosicionGuardada(save.PosX, save.PosY);

            _sceneManager.AddScene(nivel);
        }
    }
}