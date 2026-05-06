using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace capybara{
public class EscenaFinMundo : IScene 
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;

   
    private Texture2D _fondoLiso;
    
    
    private Texture2D _texBotonMundos;
    private Texture2D _texBotonMenu;
    private Rectangle _rectMundos;
    private Rectangle _rectMenu;
    private Rectangle colMundos;
    private Rectangle colMenu;

    private MouseState _estadoRatonAnterior;

    public EscenaFinMundo(SceneManager sceneManager, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _sceneManager = sceneManager;
        _content = content;
        _graphicsDevice = graphicsDevice;
    }

    public void LoadContent()
    {
        
        _fondoLiso = new Texture2D(_graphicsDevice, 1, 1);
        _fondoLiso.SetData(new[] { new Color(10, 10, 10, 180) }); 

        _texBotonMundos = _content.Load<Texture2D>("boton_seleccion_mundos");
        _texBotonMenu = _content.Load<Texture2D>("boton_menu");

        int anchoBoton = 700;
        int altoBoton = 350;
        int xCentrada = 290;
        _rectMundos = new Rectangle(xCentrada, 100, anchoBoton, altoBoton);
        _rectMenu = new Rectangle(xCentrada, 300, anchoBoton, altoBoton);

        int altoClic = 100;
        colMundos = new Rectangle(xCentrada, _rectMundos.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
        colMenu = new Rectangle(xCentrada, _rectMenu.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);

            

    }

    public void Update(GameTime gameTime)
    {
        MouseState estadoRatonActual = Mouse.GetState();

          if (MediaPlayer.State == MediaState.Playing)
            {
              MediaPlayer.Stop();
            }
        
        
        Rectangle ratonRect = new Rectangle(estadoRatonActual.X, estadoRatonActual.Y, 1, 1);

        
        bool clicIzquierdo = estadoRatonActual.LeftButton == ButtonState.Pressed && 
                             _estadoRatonAnterior.LeftButton == ButtonState.Released;

        if (clicIzquierdo)
        {
            if (colMundos.Intersects(ratonRect))
            {
                // Volver a la pantalla de selección de mundos
                while (_sceneManager.sceneaActual() is not MenuScene)
                    _sceneManager.RemoveScene();

                EscenaSeleccionMundo seleccionMundo = new EscenaSeleccionMundo(
                    _sceneManager, _content, _graphicsDevice, null);
                seleccionMundo.LoadContent();
                _sceneManager.AddScene(seleccionMundo);
            }
            else if (colMenu.Intersects(ratonRect))
            {
                // Volver al menú principal
                while (_sceneManager.sceneaActual() is not MenuScene)
                    _sceneManager.RemoveScene();
            }
        }

        
        _estadoRatonAnterior = estadoRatonActual;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        
        Rectangle pantalla = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        spriteBatch.Draw(_fondoLiso, pantalla, Color.White);

        
        MouseState raton = Mouse.GetState();
        Rectangle puntaRaton = new Rectangle(raton.X, raton.Y, 1, 1);

        
        Color colorMundos = colMundos.Intersects(puntaRaton) ? Color.Gray : Color.White;
        Color colorMenu = colMenu.Intersects(puntaRaton) ? Color.Gray : Color.White;

        spriteBatch.Draw(_texBotonMundos, _rectMundos, colorMundos);
        spriteBatch.Draw(_texBotonMenu, _rectMenu, colorMenu);
    }
}
}