using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Añadido para facilitar manejo de colecciones
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace capybara;
//Clase que va a crear los objetos de tipo juego, es decir el videojuego en si
//hereda de game

//funciona tal que Initialize()-> LoadContent() -> (Update()+Draw()) 
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Dictionary<Vector2, int> tilemap;
    private List<Rectangle> texturas;
    private Texture2D textureAtlas;
    //añadimos al personaje a la escena
    Texture2D capybara;
    //atributos esenciales para el personaje
    Vector2 position;//vamos a ir guardando su posición
    Vector2 velocity;
    
    Texture2D pixel;


    MovedSprite personaje;

    float velocidad = 4f;
    float fuerza = -8f;
    float gravedad = 0.5f;
    int saltos = 0;
    KeyboardState teclaanterior; //esto es para que si mantenemos presionado la w no haga doble salto
    float escala = 1.3f;

    List<Sprite> sprites;
    private List<BoxCollider> bloqueColliders = new List<BoxCollider>();


    public Game1()
    { 
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280; // Ancho
        _graphics.PreferredBackBufferHeight = 720; // Alto
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        tilemap = CargarMapa("Content/tilemap.csv");
        texturas = new()
        {
            new Rectangle(0,32,32,32), //1
            new Rectangle(32,32,32,32),//2
            new Rectangle(64,32,32,32),//3
            new Rectangle(96,32,32,32),//4
            new Rectangle(128,32,32,32),//5
            new Rectangle(160,32,32,32),//6
            new Rectangle(192,32,32,32),//7
            new Rectangle(0,64,32,32),  //8
            new Rectangle(32,64,32,32),//9
            new Rectangle(64,64,32,32),//10
            new Rectangle(96,64,32,32),//11
            new Rectangle(128,64,32,32),//12
            new Rectangle(160,64,32,32),//13
            new Rectangle(0,96,32,32), //14
            new Rectangle(32,96,32,32),//15
            new Rectangle(64,96,32,32),//16
            new Rectangle(96,96,32,32),//17
            new Rectangle(128,96,32,32),//18
            new Rectangle(160,96,32,32),//19
            new Rectangle(96,128,32,32),//20
            new Rectangle(128,128,32,32),//21
            new Rectangle(160,128,32,32),//22
        };

    }
private Dictionary<Vector2, int> CargarMapa(string ruta)
    {
        Dictionary<Vector2, int> resultado = new();

        StreamReader lector = new(ruta);
        int y=0;
        string linea;
        while((linea=lector.ReadLine())!= null)
        {
            string[] objetos = linea.Split(',');

            for(int x=0; x < objetos.Length; x++)
            {
                if(int.TryParse(objetos[x], out int valor))
                {
                    if (valor > 0)
                    {
                        resultado[new Vector2(x,y)]=valor;
                    }
                }
            }
            y++;
        }
        return resultado;
    }
    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        //inicializamos la posición del personaje

        base.Initialize();
    }

    protected override void LoadContent()
    {
        //esto va a cargar texturas, fuentes, sonidos...
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        
        Texture2D texturecapibara = Content.Load<Texture2D>("personaje_basico");
        personaje = new MovedSprite(texturecapibara, new Vector2(400, 300), escala, velocidad); //cargamos al capibara, pasando la textura, la posicion, la escala y la velocidad
        textureAtlas = Content.Load<Texture2D>("tilesheet");
        // TODO: use this.Content to load your game content here
        capybara = Content.Load<Texture2D>("personaje_basico"); 
        
        sprites= new();
        sprites.Add(personaje);
        
        // Añadir collider del personaje
        CollisionManager.AddCollider(personaje.Collider);
        
        // IDs de tiles que NO tienen colisión (decorativos)
        List<int> tilesSinColision = new List<int> { 18 }; //por ahora solo tenemos el fondo pero por si en un futurio queremos añadir

        // Crear colliders de los bloques
        int tileSize = 60;
        int offsetX = 0;
        int offsetY = 0;

        foreach (var item in tilemap)
        {
            // Solo añadir collider si el tile tiene colisión
            if (!tilesSinColision.Contains(item.Value))
            {
                BoxCollider bloqueCollider = new BoxCollider(
                    new Vector2(item.Key.X * tileSize + offsetX, item.Key.Y * tileSize + offsetY),
                    tileSize,
                    tileSize
                );
                bloqueColliders.Add(bloqueCollider);
                CollisionManager.AddCollider(bloqueCollider);
            }
        }
        //esto es para ver el bounding box para cuando vayamos a hacer pruebas
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });


        }
    protected override void Update(GameTime gameTime)
    {
        KeyboardState tecladoActual = Keyboard.GetState();

        //float sueloY = float.MaxValue;
        
                
        // 1. Actualizar collider del personaje
        Rectangle playerRect = personaje.Rect;
        personaje.Collider.Position = new Vector2(playerRect.X, playerRect.Y);
        personaje.Collider.Width = playerRect.Width;
        personaje.Collider.Height = playerRect.Height;

        // Actualizar manager de colisiones
        CollisionManager.Update();

        personaje.Update(tecladoActual, teclaanterior,gravedad, fuerza);



        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
            tecladoActual.IsKeyDown(Keys.Escape))
            Exit();

        List<Sprite> killist = new();    
        foreach (var sprite in sprites)
            {
                if (sprite!=personaje && sprite is ScaledSprite escalado && escalado.Rect.Intersects(personaje.Rect))
                {
                    killist.Add(sprite);
                    Console.Write("colision");
                }

            }   
        foreach (var sprite in killist)
            {
                sprites.Remove(sprite);
            } 
            // actualizacion de la tecla anterior 
            teclaanterior = tecladoActual;

            base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        //"Dibuja" es decir representa constantemente en pantalla 
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Your drawing code here
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Primero: dibujar tiles (capa de atrás)
        foreach (var item in tilemap)
        {
            
            int tileSize = 60;
            int offsetX = 0;
            int offsetY = 0;

            Rectangle dest = new(
                (int)item.Key.X * tileSize + offsetX,
                (int)item.Key.Y * tileSize + offsetY,
                tileSize,
                tileSize
            );
            Rectangle src = texturas[item.Value - 1];
            _spriteBatch.Draw(textureAtlas, dest, src, Color.White);
        }

        // Segundo: dibujar personaje (capa de adelante)
        _spriteBatch.Draw(
            personaje.texture,
            personaje.position,
            null,
            Color.White,
            0f,
            new Vector2(personaje.texture.Width / 2f, personaje.texture.Height / 2f),
            escala,
            personaje.efecto,
            0f
        );

        // Tercero: dibujar bounding box (para depuración)
        _spriteBatch.Draw(pixel, personaje.Rect, Color.Red * 0.5f);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}