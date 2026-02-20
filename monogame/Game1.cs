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
    //añadimos al personaje a la escena
    Texture2D capybara;
    //atributos esenciales para el personaje
    Vector2 position;//vamos a ir guardando su posición
    Vector2 velocity;

    float velocidad = 4f;
    float fuerza=-8f;
    float gravedad=0.5f;
    int saltos=0;
    KeyboardState teclaanterior; //esto es para que si mantenemos presionado la w no haga doble salto
    SpriteEffects efecto = SpriteEffects.None; //esto es para que se de la vuelta 
    //Si usamos el flip horizontal se cambia muy bruscamente por lo que vamos a interpolar despacito
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
        position = new Vector2(400, 300);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        //esto va a cargar texturas, fuentes, sonidos...
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        capybara = Content.Load<Texture2D>("personaje_basico"); 
    }

    protected override void Update(GameTime gameTime)
    {
        //basicamente el funcionamiento 
        //primero como cogemos el movimiento 
        KeyboardState keyboard = Keyboard.GetState();
        //ahi lo que hacemos es coger los cambios en el teclado
        // Movimiento horizontal
        // Movimiento horizontal
        if (keyboard.IsKeyDown(Keys.D))
        {
            velocity.X = velocidad;
            efecto = SpriteEffects.None; // mirar derecha
        }
        else if (keyboard.IsKeyDown(Keys.A))
        {
            velocity.X = -velocidad;
            efecto = SpriteEffects.FlipHorizontally; // mirar izquierda
        }
        else
        {
            velocity.X = 0;
        }

        // Salto
        if (keyboard.IsKeyDown(Keys.W) && saltos<2 && teclaanterior.IsKeyUp(Keys.W))//si hay suelo y le damos a la w saltamos
        {
            saltos++;
            velocity.Y = fuerza; //como es un vector 2D, el movimiento en el eje Y la componente Y del vector
            
        }

        // Aplicar gravedad
        velocity.Y += gravedad;

        // Aplicar velocidad a la posición
        position += velocity;

        // Suelo artificial (ejemplo: altura 300)
        //cuando volvemos a tocar el suelo, se reinicia
        float mitadAltura = (capybara.Height * escala) / 2f;

        if (position.Y >= 450 - mitadAltura)
        {
            position.Y = 450 - mitadAltura;
            velocity.Y = 0;
            saltos = 0;
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        teclaanterior=keyboard;
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
                capybara,
                position,
                null,
                Color.White,
                0f,
                new Vector2(capybara.Width / 2f, capybara.Height / 2f),
                escala,
                efecto,
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
