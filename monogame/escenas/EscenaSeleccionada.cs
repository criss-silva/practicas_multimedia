using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Runtime.CompilerServices;

namespace capybara
{
    public class EscenaSeleccionada : IScene
    {
        private SceneManager _sceneManager;
        private ContentManager _content;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _pixel;
        private bool _ratonSoltado = false;

        private AnimacionFondo _animacionfondo;

        private Rectangle _rectStart;
        private Rectangle _rectContinue;
        private Texture2D startTexture;
        private Texture2D continueTexture;
        
        private MouseState _mouseAnterior;

        public EscenaSeleccionada(SceneManager sm, ContentManager content, GraphicsDevice gd, AnimacionFondo animacionfondo)
        {
            _sceneManager = sm;
            _content = content;
            _graphicsDevice = gd;
            _animacionfondo = animacionfondo;
            

           
            _rectStart = new Rectangle(550, 150, 600, 400);
            _rectContinue = new Rectangle(100, 150, 600, 400);
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
            _animacionfondo.Update(gameTime);
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = new Point(mouseActual.X, mouseActual.Y);
             // para que no se mezclen los clicks entre escenas
            if (mouseActual.LeftButton == ButtonState.Released)
            {
                _ratonSoltado = true; 
            }

           
            if (_ratonSoltado)
            {
                if (_rectStart.Contains(mousePos))
                {
                    
                    if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        SaveManager.BorrarSave();
                        GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
                        nivel1.LoadContent();
                        _sceneManager.AddScene(nivel1);
                        return; 
                    }
                }
            }
            _mouseAnterior = mouseActual;
        }
            

            
           
        public void Draw(SpriteBatch spriteBatch)
        {

          
            _animacionfondo.Draw(spriteBatch, new Rectangle(0, 0, 1280, 720));
            
            spriteBatch.Draw(startTexture, _rectStart, Color.White);

            
            spriteBatch.Draw(continueTexture, _rectContinue, Color.White);
        }
    }
}