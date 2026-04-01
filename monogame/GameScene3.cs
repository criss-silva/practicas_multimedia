using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;


namespace capybara;

public class GameScene3 : IScene
{
    private Dictionary<Vector2, int> tilemap;
    private List<Rectangle> texturas;
    private Texture2D textureAtlas;
    private MovedSprite personaje;
    private List<Sprite> sprites;
    private Texture2D pixel;
    private Enemigo enemigo;
    private SceneManager _sceneManager; 
    private float escala = 1.0f;
    private float velocidad = 4f;
    private float gravedad = 0.5f;
    private float fuerza = -8f;
    private KeyboardState teclaanterior;

    private ContentManager Content;
    private GraphicsDevice _graphicsDevice;

    public GameScene3(SceneManager sm, ContentManager content, GraphicsDevice gd)
    {
        _sceneManager = sm;
        this.Content = content;
        this._graphicsDevice = gd;

        // Suscribirse a los eventos de vidas
        VidaManager.OnPerderVida += Respawn;
        VidaManager.OnGameOver += GameOver;
    }

    public void LoadContent()
    {
        CollisionManager.Clear();

        tilemap = CargarMapa("Content/nivel1.csv");
        textureAtlas = Content.Load<Texture2D>("tilesheet");
        Texture2D texturecapibara = Content.Load<Texture2D>("personaje_basico");
        Texture2D texAnimacion = Content.Load<Texture2D>("animacion_burbuja");
        Texture2D texSalida = Content.Load<Texture2D>("animacion_romper_burbuja"); 
        personaje = new MovedSprite(texturecapibara, new Vector2(100, 90), escala, velocidad, texAnimacion, texSalida);
        sprites = new List<Sprite> { personaje };
       enemigo = new Enemigo(Content.Load<Texture2D>("enemigo1"), new Vector2(800, 250), 0.1f, 2);

        CollisionManager.AddCollider(personaje.Collider);
        texturas = new()
        {
            new Rectangle(0,32,32,32), //1
             new Rectangle(32,32,32,32),  //2
             new Rectangle(64,32,32,32), //3
            new Rectangle(96,32,32,32), //4
            new Rectangle(128,32,32,32), //5
            new Rectangle(160,32,32,32), //6
            new Rectangle(192,32,32,32), //7
            new Rectangle(0,64,32,32),  //8
            new Rectangle(32,64,32,32), //9
            new Rectangle(64,64,32,32), //10
            new Rectangle(96,64,32,32), //11
            new Rectangle(128,64,32,32), //12
            new Rectangle(160,64,32,32), //13
            new Rectangle(0,96,32,32),  //14
            new Rectangle(32,96,32,32), //15
            new Rectangle(64,96,32,32), //16
            new Rectangle(96,96,32,32), //17
            new Rectangle(128,96,32,32), //18
            new Rectangle(160,96,32,32), //19
            new Rectangle(96,128,32,32), //20
            new Rectangle(128,128,32,32), //21
            new Rectangle(160,128,32,32) //22
        };


        int tileSize = 60;
        foreach (var item in tilemap)
        {
            if (item.Value != 18 && item.Value != 99 && item.Value != 50)
            {
                BoxCollider bloque = new BoxCollider(new Vector2(item.Key.X * tileSize, item.Key.Y * tileSize), tileSize, tileSize);
                CollisionManager.AddCollider(bloque);
            }
        }


        pixel = new Texture2D(_graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState tecladoActual = Keyboard.GetState();

        Rectangle playerRect = personaje.Rect;
        personaje.Collider.Position = new Vector2(playerRect.X, playerRect.Y);
        personaje.Collider.Width = playerRect.Width;
        personaje.Collider.Height = playerRect.Height;

        CollisionManager.Update();
        personaje.Update(tecladoActual, teclaanterior, gravedad, fuerza, gameTime); 
        
        enemigo.Update(gameTime, personaje, tilemap); //actualizacion del enemigo pasando la referencia del personaje
        List<Sprite> killist = new();
        foreach (var sprite in sprites)
        {
            if (sprite != personaje && sprite is ScaledSprite escalado && escalado.Rect.Intersects(personaje.Rect))
                killist.Add(sprite);
        }
        foreach (var sprite in killist) sprites.Remove(sprite);

        // Comprobar si el personaje toca un tile 99 (meta -> nivel 2)
        int tileSize = 60;
        foreach (var item in tilemap)
        {
            if (item.Value == 50)
            {
                Rectangle tileKill = new Rectangle(
                    (int)item.Key.X * tileSize,
                    (int)item.Key.Y * tileSize,
                    tileSize, tileSize);

                if (personaje.Rect.Intersects(tileKill))
                {
                    VidaManager.PerderVida();
                    enemigo.ResetearPosicion(); // para que el enemigo también vuelva a su posición inicial
                    break;
                }
            }
            if (item.Value == 99)
            {
                Rectangle tileMeta = new Rectangle(
                    (int)item.Key.X * tileSize,
                    (int)item.Key.Y * tileSize,
                    tileSize, tileSize);

                if (personaje.Rect.Intersects(tileMeta))
                {
                    CollisionManager.Clear();
                    GameScene2 nivel2 = new GameScene2(_sceneManager, Content, _graphicsDevice);
                    nivel2.LoadContent();
                    _sceneManager.AddScene(nivel2);
                    return; // salir para no seguir procesando este frame
                }
            }
        }

        teclaanterior = tecladoActual;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = 60;

        // Dibujar Tiles (ignorar el 99, no tiene textura)
        foreach (var item in tilemap)
        {
            if (item.Value == 99 || item.Value == 50) continue;

            Rectangle dest = new((int)item.Key.X * tileSize, (int)item.Key.Y * tileSize, tileSize, tileSize);
            spriteBatch.Draw(textureAtlas, dest, texturas[item.Value - 1], Color.White);
        }
        // dibujar enemigo
        enemigo.Draw(spriteBatch);

        // Dibujar Personaje
        if (personaje.EnEstadoS || personaje.SaliendoDeS)
    {
        Texture2D texAUsar = personaje.EnEstadoS ? personaje.TexEspecial : personaje.TexSalida;
        int totalFrames = personaje.EnEstadoS ? personaje.TotalFramesS : personaje.TotalFramesSalida;

       
        int anchoFrame = texAUsar.Width / totalFrames;
        int altoFrame = texAUsar.Height;
        Rectangle fuente = new Rectangle(personaje.FrameActualS * anchoFrame, 0, anchoFrame, altoFrame);
        float escalaAjustadaX = (float)personaje.texture.Width / anchoFrame;
        float escalaAjustadaY = (float)personaje.texture.Height / altoFrame;
        Vector2 escalaFinal = new Vector2(escalaAjustadaX * escala, escalaAjustadaY * escala);

        Vector2 origen = new Vector2(anchoFrame / 2f, altoFrame / 2f);

        spriteBatch.Draw(texAUsar, personaje.position, fuente, Color.White, 0f, origen, escalaFinal, personaje.efecto, 0f);
    }
        else
        {
            spriteBatch.Draw(personaje.texture, personaje.position, null, Color.White, 0f,
                new Vector2(personaje.texture.Width / 2f, personaje.texture.Height / 2f),
                escala, personaje.efecto, 0f);
        }

       
        spriteBatch.Draw(pixel, personaje.Rect, Color.Red * 0.5f);
    }

    private Dictionary<Vector2, int> CargarMapa(string ruta)
    {
        Dictionary<Vector2, int> resultado = new();
        if (!File.Exists(ruta)) return resultado;
        using StreamReader lector = new(ruta);
        int y = 0;
        string linea;
        while ((linea = lector.ReadLine()) != null)
        {
            string[] objetos = linea.Split(',');
            for (int x = 0; x < objetos.Length; x++)
            {
                if (int.TryParse(objetos[x], out int valor) && valor > 0)
                    resultado[new Vector2(x, y)] = valor;
            }
            y++;
        }
        return resultado;
    }
    private void Respawn()
    {
        personaje.position = new Vector2(100, 90);
        personaje.velocity = Vector2.Zero; // para que no siga con inercia
    }

    private void GameOver()
    {
        // Desuscribirse para no dejar eventos colgados
        VidaManager.OnPerderVida -= Respawn;
        VidaManager.OnGameOver -= GameOver;

        CollisionManager.Clear();
        GameOverScene gameOver = new GameOverScene(_sceneManager, Content, _graphicsDevice);
        gameOver.LoadContent();
        _sceneManager.AddScene(gameOver);
    }
}