using System;
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


    MovedSprite personaje;

    float velocidad = 4f;
    float fuerza = -8f;
    float gravedad = 0.5f;
    int saltos = 0;
    KeyboardState teclaanterior; //esto es para que si mantenemos presionado la w no haga doble salto
    float escala = 2f;




    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
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

    }

    protected override void Update(GameTime gameTime)
    {

        KeyboardState tecladoActual = Keyboard.GetState();


        float sueloY = GraphicsDevice.Viewport.Height;
        personaje.Update(tecladoActual, gravedad, sueloY, saltos, teclaanterior, fuerza);


        //salir del jeugo
        if (tecladoActual.IsKeyDown(Keys.Escape))
            Exit();

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

        //queremos pintar capybara en la posición del vector y color blanco para que no se tinte
        //normalmente se usa Texture2D, vector, color. Pero usamos rectangle para poder redimensionar el objeto
        //si la ampliamos se va a ver borroso por lo que vamos a ver el mapping, para ello en begin usamos
        //samplerState: SamplerState.PointClamp (cogemos los pixeles cercanos)

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}