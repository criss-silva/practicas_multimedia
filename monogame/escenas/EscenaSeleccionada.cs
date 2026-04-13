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

            // Hover y click en Start
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

            // Hover en Continue (sin funcionalidad aún)
            if (_rectContinue.Contains(mousePos))
                _colorContinue = Color.DimGray;
            else
                _colorContinue = Color.Gray;

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
    }
}