using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input; // Añadimos esto para el ratón

namespace capybara;

public class GameOverScene : IScene
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;
    private Texture2D _pixel;

    // Botones: Texturas, Rectángulos y Colores (para el efecto hover)
    private Texture2D _texReiniciar, _texMenu;
    private Rectangle _rectReiniciar, _rectMenu;
    private Color _colorReiniciar = Color.White, _colorMenu = Color.White, _colorSalir = Color.White;

    // Animación
    private Texture2D _spriteSheet;
    private int _columnas = 3;
    private int _filas = 1;
    private int _frameActual = 0;
    private float _tiempoPorFrame = 0.15f;
    private float _cronometro = 0f;
    private Vector2 _posAnimacion;

    public GameOverScene(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        _content = content;
        _graphicsDevice = gd;
    }

    public void LoadContent()
    {
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

       //animacion
        _spriteSheet = _content.Load<Texture2D>("animacion_perder");
         _posAnimacion = new Vector2(1280 / 2 + 50, 720 / 2 - 150); 

         //botones
        float escala = 0.5f; 

        _texReiniciar = _content.Load<Texture2D>("boton_restart");
        _texMenu = _content.Load<Texture2D>("boton_menu");
        float ajusteAncho = 0.7f; 
        float ajusteAlto = 0.6f;

    
    int anchoR = (int)(_texReiniciar.Width * escala * ajusteAncho);
    int altoR = (int)(_texReiniciar.Height * escala * ajusteAlto);
    _rectReiniciar = new Rectangle(
        1280 / 2 - (anchoR / 2), 
        200, 
        anchoR, 
        altoR
    );
    int anchoM = (int)(_texMenu.Width * escala * ajusteAncho);
    int altoM = (int)(_texMenu.Height * escala * ajusteAlto);
    _rectMenu = new Rectangle(
        1280 / 2 - (anchoM / 2), 
        350, 
        anchoM, 
        altoM
    );

    
        
    }

    public void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        Point mousePos = new Point(mouse.X, mouse.Y);

        //animacion con cronometro
        _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cronometro >= _tiempoPorFrame)
        {
            _frameActual = (_frameActual + 1) % (_columnas * _filas);
            _cronometro = 0f;
        }
            //logica boton reiniciar
        if (_rectReiniciar.Contains(mousePos))
        {
            _colorReiniciar = Color.Gray;
            if (mouse.LeftButton == ButtonState.Pressed) ReiniciarNivel();
        }
        else _colorReiniciar = Color.White;

        // logica boton menu
        if (_rectMenu.Contains(mousePos))
        {
            _colorMenu = Color.Gray;
            if (mouse.LeftButton == ButtonState.Pressed) IrAlMenu();
        }
        else _colorMenu = Color.White;

    }

    private void ReiniciarNivel()
{
    //limpiamos toda la pila
    while (_sceneManager.sceneaActual() is not MenuScene)
    {
        _sceneManager.RemoveScene();
    }
    //reseteamos colisiones y vidas
    CollisionManager.Clear();
    VidaManager.Resetear();

    //creamos nuea instancia de gamescene para que se vuelvan a generar los colisionadores
    GameScene nivel1 = new GameScene(_sceneManager, _content, _graphicsDevice);
    nivel1.LoadContent(); 
    
    _sceneManager.AddScene(nivel1);
}
    private void IrAlMenu()
    {
        while (_sceneManager.sceneaActual() is not MenuScene)
            _sceneManager.RemoveScene();
        CollisionManager.Clear();
        VidaManager.Resetear();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
    
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Red * 0.7f);

        // Dibujar Botones
        spriteBatch.Draw(_texReiniciar, _rectReiniciar, _colorReiniciar);
        spriteBatch.Draw(_texMenu, _rectMenu, _colorMenu);
        

        // Dibujar Animación
        int w = _spriteSheet.Width / _columnas;
        int h = _spriteSheet.Height / _filas;
        Rectangle fuente = new Rectangle((_frameActual % _columnas) * w, (_frameActual / _columnas) * h, w, h);
        
        spriteBatch.Draw(_spriteSheet, _posAnimacion, fuente, Color.White, 0f, 
            new Vector2(w / 2, h / 2), 0.3f, SpriteEffects.None, 0f);
    }
}