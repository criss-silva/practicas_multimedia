using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara;

public class EscenaSeleccionMundo : IScene
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;

        /// <summary>
       /// fondo animado para la pantalla de inicio
       /// </summary>
        private AnimacionFondo _animacionFondo;
        /// <summary>Textura de fondo que ocupa toda la pantalla.</summary>
        private Texture2D _fondo;

    private Texture2D _pixel;
    private Rectangle _rectMundo1;
    private Rectangle _rectMundo2;
    private Rectangle _rectVolver;
    private MouseState _mouseAnterior;

    public EscenaSeleccionMundo(SceneManager sm, ContentManager content, GraphicsDevice gd, AnimacionFondo animacionfondo = null)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;
        _animacionFondo = animacionfondo;

        _rectMundo1 = new Rectangle(150, 180, 400, 300);
        _rectMundo2 = new Rectangle(720, 180, 400, 300);
        _rectVolver = new Rectangle(490, 560, 300, 80);
    }

    public void LoadContent()
    {
        Texture2D sheetMenu = _content.Load<Texture2D>("fondo_burbuja");
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        // Siempre se crea una instancia nueva para garantizar que no sea null
        _animacionFondo = new AnimacionFondo(sheetMenu, totalFrames: 16, fps: 7f);
    }

    public void Update(GameTime gameTime)
    {
        _animacionFondo.Update(gameTime);
        MouseState mouseActual = Mouse.GetState();
        Point mousePos = new Point(mouseActual.X, mouseActual.Y);
        bool clic = mouseActual.LeftButton == ButtonState.Pressed
                 && _mouseAnterior.LeftButton == ButtonState.Released;

        if (clic)
        {
            if (_rectMundo1.Contains(mousePos))
            {
                CollisionManager.Clear();
                VidaManager.Resetear();
                IScene nivel = new GameScene(_sceneManager, _content, _graphicsDevice);
                nivel.LoadContent();
                _sceneManager.AddScene(nivel);
            }
            else if (_rectMundo2.Contains(mousePos))
            {
                CollisionManager.Clear();
                VidaManager.Resetear();
                IScene nivel = new GameScene_M2(_sceneManager, _content, _graphicsDevice);
                nivel.LoadContent();
                _sceneManager.AddScene(nivel);
            }
            else if (_rectVolver.Contains(mousePos))
            {
                // Volver al menú principal
                while (_sceneManager.sceneaActual() is not MenuScene)
                    _sceneManager.RemoveScene();
            }
        }

        _mouseAnterior = mouseActual;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _animacionFondo.Draw(spriteBatch, new Rectangle(0, 0, 1280, 720));

        // Cuadrado Mundo 1
        bool hoverM1 = _rectMundo1.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_pixel, _rectMundo1, hoverM1 ? Color.CornflowerBlue : Color.SteelBlue);

        // Borde Mundo 1
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.X, _rectMundo1.Y, _rectMundo1.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.X, _rectMundo1.Bottom - 4, _rectMundo1.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.X, _rectMundo1.Y, 4, _rectMundo1.Height), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.Right - 4, _rectMundo1.Y, 4, _rectMundo1.Height), Color.White);

        // Cuadrado Mundo 2
        bool hoverM2 = _rectMundo2.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_pixel, _rectMundo2, hoverM2 ? Color.MediumSeaGreen : Color.SeaGreen);

        // Borde Mundo 2
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.X, _rectMundo2.Y, _rectMundo2.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.X, _rectMundo2.Bottom - 4, _rectMundo2.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.X, _rectMundo2.Y, 4, _rectMundo2.Height), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.Right - 4, _rectMundo2.Y, 4, _rectMundo2.Height), Color.White);

        // Botón volver
        bool hoverV = _rectVolver.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_pixel, _rectVolver, hoverV ? Color.IndianRed : Color.Firebrick);
    }
}