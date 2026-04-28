using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara;

public class EscenaFinMundo : IScene
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;

    private AnimacionFondo _animacionFondo;

    private Texture2D _pixel;
    private Rectangle _rectElegir;
    private Rectangle _rectSalir;
    private MouseState _mouseAnterior;

    public EscenaFinMundo(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;

        _rectElegir = new Rectangle(390, 300, 500, 100);
        _rectSalir  = new Rectangle(390, 450, 500, 100);
    }

    public void LoadContent()
    {
        Texture2D sheetMenu = _content.Load<Texture2D>("fondo_burbuja");   
        _animacionFondo = new AnimacionFondo(sheetMenu, totalFrames: 16, fps: 7f);
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
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
            if (_rectElegir.Contains(mousePos))
            {
                CollisionManager.Clear();
                VidaManager.Resetear();
                // Limpiar hasta llegar al menú y lanzar selección de mundo
                while (_sceneManager.sceneaActual() is not MenuScene)
                    _sceneManager.RemoveScene();

                EscenaSeleccionMundo seleccionMundo = new EscenaSeleccionMundo(
                    _sceneManager, _content, _graphicsDevice, _animacionFondo);
                seleccionMundo.LoadContent();
                _sceneManager.AddScene(seleccionMundo);
            }
            else if (_rectSalir.Contains(mousePos))
            {
                CollisionManager.Clear();
                VidaManager.Resetear();
                while (_sceneManager.sceneaActual() is not MenuScene)
                    _sceneManager.RemoveScene();
            }
        }

        _mouseAnterior = mouseActual;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Black * 0.85f);

        bool hoverE = _rectElegir.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_pixel, _rectElegir, hoverE ? Color.CornflowerBlue : Color.SteelBlue);

        bool hoverS = _rectSalir.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_pixel, _rectSalir, hoverS ? Color.IndianRed : Color.Firebrick);
    }
}