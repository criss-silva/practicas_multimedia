using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace capybara;

public class WinScene : IScene
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;
    private Texture2D _pixel; 
    
    
    private Texture2D _texturaBoton;
    private Rectangle _rectBoton;
    private Color _tinteBoton = Color.White; 

//para la animacion

    private Texture2D _spriteSheet;
    private int _columnas = 3;        
    private int _filas = 1;           
    private int _totalFrames;
    private int _frameActual = 0;
    private float _tiempoPorFrame = 0.15f;  // 15 fps
    private float _cronometro = 0f;

    private Vector2 _posAnimacion;

    public WinScene(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;
        
      
    }

    public void LoadContent()
    {

            //carga animacion
        _spriteSheet = _content.Load<Texture2D>("animacion_ganar");
        _totalFrames = _columnas * _filas;
        _posAnimacion = new Vector2(1280 / 2, 720 / 2 - 150);        
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        
        _texturaBoton = _content.Load<Texture2D>("boton_menu"); 

        
        int anchoFinal=600;
        int altoFinal=300;

       
        int posX = (1280 / 2) - (anchoFinal / 2); 
        int posY = 300; 

        _rectBoton = new Rectangle(posX, posY, anchoFinal, altoFinal);
    }

    public void Update(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();
        Point mousePosition = new Point(mouseState.X, mouseState.Y);

        //usamos un cronometro para que vaya poniendo la animacion segun los segundos que le hemos puesto, si llega a mas de 3 escenas se reinicia
         _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_cronometro >= _tiempoPorFrame)
    {
        _frameActual++;
        if (_frameActual >= _totalFrames) _frameActual = 0; 
        _cronometro = 0f;
    }
        //comprobamos si el raton esta en el boton
        if (_rectBoton.Contains(mousePosition))
        {
            _tinteBoton = Color.Gray; 

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                RegresarAlMenu();
            }
        }
        else
        {
            _tinteBoton = Color.White; 
        }
    }

    private void RegresarAlMenu()
    {
       //para volver al menu limpiando la pila
        while (_sceneManager.sceneaActual() is not MenuScene)
            _sceneManager.RemoveScene();
        
        CollisionManager.Clear();
        VidaManager.Resetear();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Gold * 0.8f);
        spriteBatch.Draw(_texturaBoton, _rectBoton, _tinteBoton);


        //dibujo de la animacion cortando el sprite sheet
        int anchoFrame = _spriteSheet.Width / _columnas; 
        int altoFrame = _spriteSheet.Height / _filas;

        int columna = _frameActual % _columnas;
        int fila = _frameActual / _columnas;

        Rectangle fuente = new Rectangle(
            columna * anchoFrame, 
            fila * altoFrame, 
            anchoFrame, 
            altoFrame
        );

        
        float escalaPequeña = 0.2f; 
        Vector2 centroFrame = new Vector2(anchoFrame / 2, altoFrame / 2);

        spriteBatch.Draw(
            _spriteSheet, 
            _posAnimacion,   
            fuente,         
            Color.White, 
            0f, 
            centroFrame, 
            escalaPequeña,   
            SpriteEffects.None, 
            0f
        );
    }
}