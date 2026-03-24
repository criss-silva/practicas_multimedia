using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace capybara;

public class GameOverScene : IScene
{
    private SceneManager _sceneManager;
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;
    private Texture2D _pixel;
    private float _timer = 0f;
    private const float DURACION = 5f; //por ahora tenemos 5 segundos para meter luego la animación 

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
    }

    public void Update(GameTime gameTime)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds; //para reproducir el tiempo designado
        if (_timer >= DURACION)
        {
            // Limpiar toda la pila hasta el menú
            while (_sceneManager.sceneaActual() is not MenuScene)
                _sceneManager.RemoveScene();

            // Resetear vidas y lanzar fase 1
            VidaManager.Resetear();
            CollisionManager.Clear();
            GameScene fase1 = new GameScene(_sceneManager, _content, _graphicsDevice);
            fase1.LoadContent();
            _sceneManager.AddScene(fase1);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Pantalla roja con transparencia
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Red * 0.7f);
    }
}