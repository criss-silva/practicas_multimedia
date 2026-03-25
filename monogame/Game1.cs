using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace capybara;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager sceneManager;
    private HUD _hud;

    public Game1()
    { 
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
        
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        sceneManager = new SceneManager();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _hud = new HUD();              
        _hud.LoadContent(Content);     

        
        // iniciamos con el menu
        MenuScene menu = new MenuScene(sceneManager, Content, GraphicsDevice);
        menu.LoadContent();
        sceneManager.AddScene(menu);
    }

    protected override void Update(GameTime gameTime)
    {
        //salir
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        //actualizamos con la escena que este en la pila arriba
        sceneManager.sceneaActual()?.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(111, 94, 132)); // verde bosque, por ejemplo;

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
        // dibujamos la escena que haya en ese momento
        sceneManager.sceneaActual()?.Draw(_spriteBatch);
        _hud.Draw(_spriteBatch); 

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}