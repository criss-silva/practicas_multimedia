using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace capybara{

public class MenuScene : IScene
{
    private Texture2D _fondo;
    private Texture2D _nombreJuego;
    private Texture2D boton_jugar;
    private Texture2D boton_ajustes;

    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;

    private Rectangle rect_jugar;
    private Rectangle rect_ajustes;
    private Rectangle rect_nombre;

    //rectangulos para detectar colisiones con los botones
    private Rectangle col_jugar;
    private Rectangle col_ajustes; 
    private MouseState _mouseAnterior;

    public MenuScene(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;

        //posicion de los  btones
        
        int anchoBoton = 600;
        int altoBoton = 300;
        int xCentrada = 330;
        rect_jugar = new Rectangle(xCentrada, 220, anchoBoton, altoBoton);
        rect_ajustes = new Rectangle(xCentrada, 375, anchoBoton, altoBoton);

        int altoClic = 100; 
        col_jugar = new Rectangle(xCentrada, rect_jugar.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);
        col_ajustes = new Rectangle(xCentrada, rect_ajustes.Y + (altoBoton / 2) - (altoClic / 2), anchoBoton, altoClic);


        int anchoNombre = 800;
        int altoNombre = 600;
        int xNombreCentrada = (1280 - anchoNombre) / 2;
        int yNombre = rect_jugar.Y - altoNombre +300;
        rect_nombre = new Rectangle(xNombreCentrada, yNombre, anchoNombre, altoNombre);
    }

    public void LoadContent()
    {
        //añadimos fondo
        _fondo = _content.Load<Texture2D>("fondo");

        //añadimos nombre del juego
        _nombreJuego = _content.Load<Texture2D>("nombre_juego");
        //añadimos botones
        boton_jugar = _content.Load<Texture2D>("boton_jugar");
        boton_ajustes = _content.Load<Texture2D>("boton_ajustes");
    }

    public void Update(GameTime gameTime)
    {

        MouseState mouseActual = Mouse.GetState();
        Point mousePos = new Point(mouseActual.X, mouseActual.Y);

        //logica para jugar

        if (col_jugar.Contains(mousePos))
        {
            if (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
            {
                GameScene juego = new GameScene(_sceneManager,_content, _graphicsDevice);
                juego.LoadContent();
                _sceneManager.AddScene(juego);
            }
        }
        _mouseAnterior=mouseActual;
        
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      // dibujar fondo
        spriteBatch.Draw(_fondo, new Rectangle(0, 0, 1280, 720), Color.White);

        // dibujar nombre del juego
        spriteBatch.Draw(_nombreJuego, rect_nombre, Color.White);
        // dibujar jugar 
       Color colorJugar = col_jugar.Contains(Mouse.GetState().Position) ? Color.LightGray : Color.White;
        spriteBatch.Draw(boton_jugar, rect_jugar, colorJugar);

        //dibujar ajustes
        Color colorSalir = col_ajustes.Contains(Mouse.GetState().Position) ? Color.LightGray : Color.White;
        spriteBatch.Draw(boton_ajustes, rect_ajustes, colorSalir);
    }
}
}