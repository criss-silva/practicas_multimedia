using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace capybara;

internal class EscenaPausa : IScene 
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;

   
    private Texture2D _fondoLiso;
    
    
    private Texture2D _texBotonReanudar;
    private Texture2D _texBotonMenu;
    private Rectangle _rectReanudar;
    private Rectangle _rectMenu;
    private Rectangle colReanudar;
    private Rectangle colMenu;

    private MouseState _estadoRatonAnterior;

    public EscenaPausa(SceneManager sceneManager, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _sceneManager = sceneManager;
        _content = content;
        _graphicsDevice = graphicsDevice;
    }

    public void LoadContent()
    {
        
        _fondoLiso = new Texture2D(_graphicsDevice, 1, 1);
        _fondoLiso.SetData(new[] { new Color(10, 10, 10, 180) }); 

        _texBotonReanudar = _content.Load<Texture2D>("boton_reanudar");
        _texBotonMenu = _content.Load<Texture2D>("boton_menu");

        int anchoPantalla = _graphicsDevice.Viewport.Width;
        int altoPantalla = _graphicsDevice.Viewport.Height;
        
        int anchoBoton = 700;
        int altoBoton = 350;
        int xCentrada = 290;
        _rectReanudar = new Rectangle(xCentrada, 100, anchoBoton, altoBoton);
        _rectMenu = new Rectangle(xCentrada, 300, anchoBoton, altoBoton);

         int altoClic = 100;
        colReanudar = new Rectangle(xCentrada, _rectReanudar.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
        colMenu = new Rectangle(xCentrada, _rectMenu.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);

            

    }

    public void Update(GameTime gameTime)
    {
        MouseState estadoRatonActual = Mouse.GetState();
        
        
        Rectangle ratonRect = new Rectangle(estadoRatonActual.X, estadoRatonActual.Y, 1, 1);

        
        bool clicIzquierdo = estadoRatonActual.LeftButton == ButtonState.Pressed && 
                             _estadoRatonAnterior.LeftButton == ButtonState.Released;

        if (clicIzquierdo)
        {
           
            if (_rectReanudar.Intersects(ratonRect))
            {
              
                _sceneManager.RemoveScene(); 
            } 
           
            else if (_rectMenu.Intersects(ratonRect))
            {
                
                
                MenuScene menu = new MenuScene(_sceneManager, _content, _graphicsDevice);
                menu.LoadContent();
                
                
                _sceneManager.AddScene(menu); 
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

        
        Color colorReanudar = colReanudar.Intersects(puntaRaton) ? Color.Gray : Color.White;
        Color colorMenu = colMenu.Intersects(puntaRaton) ? Color.Gray : Color.White;

        spriteBatch.Draw(_texBotonReanudar, _rectReanudar, colorReanudar);
        spriteBatch.Draw(_texBotonMenu, _rectMenu, colorMenu);
    }
}