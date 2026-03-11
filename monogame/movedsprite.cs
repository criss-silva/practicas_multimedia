using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace capybara
{
    internal class MovedSprite : ScaledSprite
    {
        private float speed;
        public new Vector2 velocity; // 'new' para evitar el warning CS0108
        private KeyboardState prevKeyboard;
        public int saltos = 0;
        
        private float escalaX = 1f;
        private float escalaObjetivo = 1f;
        private float velocidadFlip = 8f;

        public MovedSprite(Texture2D texture, Vector2 position, float scale, float speed)
            : base(texture, position, scale)
        {
            this.speed = speed;
            this.velocity = Vector2.Zero;
        }

        public void Update(KeyboardState keyboard, float gravedad, float sueloY,  KeyboardState teclaanterior, float fuerza)
        {
            // Movimiento horizontal
            if (keyboard.IsKeyDown(Keys.D))
            {
                velocity.X = speed;
                escalaObjetivo = 1f;
                efecto = SpriteEffects.None; // Mirar a la derecha
            }
            else if (keyboard.IsKeyDown(Keys.A))
            {
                velocity.X = -speed;
                escalaObjetivo = -1f;
                efecto = SpriteEffects.FlipHorizontally; // Mirar a la izquierda
            }
            else
            {
                velocity.X = 0;
            }

            // Flip suave
            escalaX += (escalaObjetivo - escalaX) * velocidadFlip * 0.016f;

            // Salto doble
            if (keyboard.IsKeyDown(Keys.W) && saltos < 2 && teclaanterior.IsKeyUp(Keys.W))
            {
                saltos++;
                velocity.Y = fuerza;
            }

            // Gravedad y movimiento
            velocity.Y += gravedad;
            position += velocity;

            // Suelo
            float mitadAltura = (Rect.Height) / 2f;
            if (position.Y >= sueloY - mitadAltura)
            {
                position.Y = sueloY - mitadAltura;
                velocity.Y = 0;
                saltos = 0;
            }

            prevKeyboard = keyboard;
        }

public void ResolverColisiones(List<Rectangle> bloques)
{
    foreach (var bloque in bloques)
    {
        if (this.Rect.Intersects(bloque))
        {
            Rectangle interseccion = Rectangle.Intersect(this.Rect, bloque);

            // colisiones verticales
            if (interseccion.Width > interseccion.Height)
            {
                if (this.Rect.Center.Y < bloque.Center.Y) //encima del suelo
                {
                    this.position.Y = bloque.Top - (this.altoCaja / 2f) - this.offsetY;
                    this.velocity.Y = 0; 
                    this.saltos = 0;
                    
                }
                else // Estamos debajo (Techo)
                {
                    this.position.Y += interseccion.Height;
                    this.velocity.Y = 0;
                }
            }
            //esto para las paredes
            else
            {
                if (this.position.X < bloque.X) 
                    this.position.X -= interseccion.Width;
                else 
                    this.position.X += interseccion.Width;
            }
        }
    }
}
}
}