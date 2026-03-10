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
    float escala = 2f;

    List<Sprite> sprites;

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

        //esto es para ver el bounding box para cuando vayamos a hacer pruebas
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });


        }
    protected override void Update(GameTime gameTime)
    {
        KeyboardState tecladoActual = Keyboard.GetState();

        float sueloY = GraphicsDevice.Viewport.Height;
        
      
        personaje.Update(tecladoActual, gravedad, sueloY, ref saltos, teclaanterior, fuerza);

        
        List<Rectangle> bloquesColision = new List<Rectangle>();
        int tileSize = 96;
        int offsetX = 0;
        int offsetY = 100;

        foreach (var item in tilemap)
        {
            bloquesColision.Add(new Rectangle(
                (int)item.Key.X * tileSize + offsetX,
                (int)item.Key.Y * tileSize + offsetY,
                tileSize,
                tileSize
            ));
        }

        //resolvemos las colisiones del personaje con los bloques de colisión
        personaje.ResolverColisiones(bloquesColision);

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

        // TODO: Add your drawing code here
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        //todo lo que hay aquí en mitad se dibuja, tiene que tener siempre un inicio y un fin

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
        // En Draw, después de dibujar el personaje
        _spriteBatch.Draw(pixel, personaje.Rect, Color.Red * 0.5f); // box rojo semitransparente

        //queremos pintar capybara en la posición del vector y color blanco para que no se tinte
        //normalmente se usa Texture2D, vector, color. Pero usamos rectangle para poder redimensionar el objeto
        //si la ampliamos se va a ver borroso por lo que vamos a ver el mapping, para ello en begin usamos
        //samplerState: SamplerState.PointClamp (cogemos los pixeles cercanos)

        foreach (var item in tilemap)
        {
            
            int tileSize = 96;
            int offsetX = 0;
            int offsetY = 100;

            Rectangle dest = new(
                (int)item.Key.X * tileSize + offsetX,
                (int)item.Key.Y * tileSize + offsetY,
                tileSize,
                tileSize
            );
            Rectangle src = texturas[item.Value - 1];
            _spriteBatch.Draw(textureAtlas, dest, src, Color.White);
        }  
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}