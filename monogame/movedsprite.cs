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
        public Vector2 velocity;
        private KeyboardState prevKeyboard;
        float escala = 2f;
        
        // Para suavizar el cambio de dirección
        private float escalaX = 1f;        // escala visual horizontal (1 o -1)
        private float escalaObjetivo = 1f; // hacia donde queremos ir
        private float velocidadFlip = 8f;  // qué tan rápido cambia

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

            // Interpolación suave del flip (en lugar de cambio brusco)
            escalaX += (escalaObjetivo - escalaX) * velocidadFlip * 0.016f;
            if (escalaX < 0)
                efecto = SpriteEffects.FlipHorizontally;
            else
                efecto = SpriteEffects.None;

            // Salto (máximo doble salto)
            if (keyboard.IsKeyDown(Keys.W) && saltos < 2 && teclaanterior.IsKeyUp(Keys.W))
            {
                saltos++;
                velocity.Y = fuerza;
            }

            // Gravedad
            velocity.Y += gravedad;

            // Aplicar movimiento
            position += velocity;

            // Suelo
            float mitadAltura = (Rect.Height * escala) / 2f;
            if (position.Y >= 450 - mitadAltura)
        {
            position.Y = 450 - mitadAltura;
            velocity.Y = 0;
            saltos = 0;
        }
        

            prevKeyboard = keyboard;
        }
    }
}