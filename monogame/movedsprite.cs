using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace capybara
{
    internal class MovedSprite : ScaledSprite
    {
        public SpriteEffects efecto = SpriteEffects.None;
        private float speed;
        public new Vector2 velocity; // 'new' para evitar el warning CS0108
        private KeyboardState prevKeyboard;
        
        private float escalaX = 1f;
        private float escalaObjetivo = 1f;
        private float velocidadFlip = 8f;

        public MovedSprite(Texture2D texture, Vector2 position, float scale, float speed)
            : base(texture, position, scale)
        {
            this.speed = speed;
            this.velocity = Vector2.Zero;
        }

        public void Update(KeyboardState keyboard, float gravedad, float sueloY, ref int saltos, KeyboardState teclaanterior, float fuerza)
        {
            // Movimiento horizontal
            if (keyboard.IsKeyDown(Keys.D))
            {
                velocity.X = speed;
                escalaObjetivo = 1f;
            }
            else if (keyboard.IsKeyDown(Keys.A))
            {
                velocity.X = -speed;
                escalaObjetivo = -1f;
            }
            else
            {
                velocity.X = 0;
            }

            // Flip suave
            escalaX += (escalaObjetivo - escalaX) * velocidadFlip * 0.016f;
            efecto = velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

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
                if (this.position.Y < bloque.Y) 
                {
                    this.position.Y -= interseccion.Height;
                    this.velocity.Y = 0; // esto lo pongo para la gravedad porque sino empuja hacia abajo y se empieza a rallar
                    
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