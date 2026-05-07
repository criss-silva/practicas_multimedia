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

    private Texture2D _pixel;
    private Rectangle _rectMundo1;
    private Rectangle _rectMundo2;
    private Rectangle _rectVolver;
    private Texture2D _texturaMundo1;
    private Texture2D _texturaMundo2;
    private Texture2D _texturaVolver;

    private MouseState _mouseAnterior;
    private SpriteFont _fuente;
 
    public EscenaSeleccionMundo(SceneManager sm, ContentManager content, GraphicsDevice gd, AnimacionFondo animacionfondo = null)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;
        _animacionFondo = animacionfondo;

        _rectMundo1 = new Rectangle(150, 180, 400, 300);
        _rectMundo2 = new Rectangle(720, 180, 400, 300);
        _rectVolver = new Rectangle(340, 450, 600, 300);
    }

    public void LoadContent()
    {
        Texture2D sheetMenu = _content.Load<Texture2D>("fondo_burbuja");
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        // Siempre se crea una instancia nueva para garantizar que no sea null
        _animacionFondo = new AnimacionFondo(sheetMenu, totalFrames: 16, fps: 7f);
        _texturaVolver = _content.Load<Texture2D>("boton_menu");
        _texturaMundo1 = _content.Load<Texture2D>("escena1");
        _texturaMundo2 = _content.Load<Texture2D>("escena2");
        try { _fuente = _content.Load<SpriteFont>("fuente"); }
            catch { _fuente = null; }

        
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
        spriteBatch.Draw(_texturaMundo1, _rectMundo1, hoverM1 ? Color.Gray : Color.White);
        DibujarTexto(spriteBatch, "Mundo 1", 720, 200);
        

        // Borde Mundo 1
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.X, _rectMundo1.Y, _rectMundo1.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.X, _rectMundo1.Bottom - 4, _rectMundo1.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.X, _rectMundo1.Y, 4, _rectMundo1.Height), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo1.Right - 4, _rectMundo1.Y, 4, _rectMundo1.Height), Color.White);

        // Cuadrado Mundo 2
        bool hoverM2 = _rectMundo2.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_texturaMundo2, _rectMundo2, hoverM2 ? Color.Gray : Color.White);
        DibujarTexto(spriteBatch, "Mundo 2",1850, 200);

        // Borde Mundo 2
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.X, _rectMundo2.Y, _rectMundo2.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.X, _rectMundo2.Bottom - 4, _rectMundo2.Width, 4), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.X, _rectMundo2.Y, 4, _rectMundo2.Height), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(_rectMundo2.Right - 4, _rectMundo2.Y, 4, _rectMundo2.Height), Color.White);

        // Botón volver
        bool hoverV = _rectVolver.Contains(Mouse.GetState().Position);
        spriteBatch.Draw(_texturaVolver, _rectVolver, hoverV ? Color.Gray : Color.White);
    }


    private void DibujarTexto(SpriteBatch sb, string texto, int w, int h)
        {
            if (_fuente != null)
            {
                Vector2 tamTexto = _fuente.MeasureString(texto);
                Vector2 posTexto = new Vector2((w - tamTexto.X) / 2f, h - tamTexto.Y - 30f);
                // Sombra
                sb.DrawString(_fuente, texto, posTexto + new Vector2(2, 2), Color.Black * 0.6f);
                sb.DrawString(_fuente, texto, posTexto, Color.White);
            }
           
        }
}