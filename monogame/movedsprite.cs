using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

//clase para el movimiento del personaje
namespace capybara
{
    internal class MovedSprite : ScaledSprite
    {
     public SpriteEffects efecto = SpriteEffects.None; //lo añadimos para el cambio de sentido del capibara
    private float speed;
    public Vector2 velocity; 
    private KeyboardState prevKeyboard;

    public MovedSprite(Texture2D texture, Vector2 position, float scale, float speed) 
        : base(texture, position, scale)
    {
        this.speed = speed;
        this.velocity = Vector2.Zero;
    }
    public void Update(KeyboardState keyboard, float gravedad, float sueloY, int saltos, KeyboardState teclaanterior, float fuerza) //update con la logica del teclado
    {
        // mov izq y der
        if (keyboard.IsKeyDown(Keys.D)){
            velocity.X = speed;
            efecto = SpriteEffects.None;
        }
        else if (keyboard.IsKeyDown(Keys.A)){
            efecto = SpriteEffects.FlipHorizontally;
            velocity.X = -speed;
        }
        else
            velocity.X = 0;

       // salto
       if (keyboard.IsKeyDown(Keys.W) && saltos<2 && teclaanterior.IsKeyUp(Keys.W))//si hay suelo y le damos a la w saltamos
        {
            saltos++;
            velocity.Y = fuerza; //como es un vector 2D, el movimiento en el eje Y la componente Y del vector
            
        }
        
        velocity.Y += gravedad;
        position += velocity;

        // Suelo
        float mitadAltura = Rect.Height / 2f;
        if (position.Y >= sueloY - mitadAltura)
        {
            position.Y = sueloY - mitadAltura;
            velocity.Y = 0;
        }

        prevKeyboard = keyboard;
    }
}
}