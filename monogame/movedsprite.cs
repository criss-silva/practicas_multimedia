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
        public new Vector2 velocity;
        private KeyboardState prevKeyboard;
        public int saltos = 0;
        
        private float escalaX = 1f;
        private float escalaObjetivo = 1f;
        private float velocidadFlip = 8f;
        public BoxCollider Collider { get; set; }

        private float offsetCompensacionGiro = 20f; // Compensa el desplazamiento del bounding box al girar

        public MovedSprite(Texture2D texture, Vector2 position, float scale, float speed)
            : base(texture, position, scale)
        {
            this.speed = speed;
            this.velocity = Vector2.Zero;
            if (Collider == null)
            {
                Collider = new BoxCollider(position, 70, 70, false);
            }
        }

            public void Update(KeyboardState keyboard, KeyboardState teclaanterior, float gravedad, float fuerza)
    {
        // Detectar cambio de dirección para compensar el offset del bounding box
        SpriteEffects efectoAnterior = efecto;

        // Movimiento horizontal
        if (keyboard.IsKeyDown(Keys.D))
        {
            velocity.X = speed;
            escalaObjetivo = 1f;
            efecto = SpriteEffects.None;
        }
        else if (keyboard.IsKeyDown(Keys.A))
        {
            velocity.X = -speed;
            escalaObjetivo = -1f;
            efecto = SpriteEffects.FlipHorizontally;
        }
        else
        {
            velocity.X = 0;
        }

        // Compensar desplazamiento al girar de derecha a izquierda
        if (efectoAnterior == SpriteEffects.None && efecto == SpriteEffects.FlipHorizontally)
        {
            position.X -= offsetCompensacionGiro;
        }
        // Compensar desplazamiento al girar de izquierda a derecha
        else if (efectoAnterior == SpriteEffects.FlipHorizontally && efecto == SpriteEffects.None)
        {
            position.X += offsetCompensacionGiro;
        }

        // Flip suave
        escalaX += (escalaObjetivo - escalaX) * velocidadFlip * 0.016f;

        // Salto doble
        if (keyboard.IsKeyDown(Keys.W) && saltos < 2 && teclaanterior.IsKeyUp(Keys.W))
        {
            saltos++;
            velocity.Y = fuerza;
        }

        // Gravedad
        velocity.Y += gravedad;


        // 1. pos y
        float oldPosY = position.Y;

        // 2. movemos en y
        position.Y += velocity.Y;
        ActualizarCollider();

        // 3. colision Y → REVERTIR
        if (HayColisionY())
        {
            position.Y = oldPosY;
            ActualizarCollider();
            
            // Si venía cayendo, tocar suelo
            if (velocity.Y > 0)
                saltos = 0;
                
            velocity.Y = 0;
        }

        //lo mismo pero con x
        float oldPosX = position.X;

        
        position.X += velocity.X;
        ActualizarCollider();

        if (HayColisionY())
        {
            position.X = oldPosX;
            ActualizarCollider();
        }

        prevKeyboard = keyboard;
    }

    private void ActualizarCollider()
    {
        if (Collider != null)
        {
            Rectangle r = this.Rect;//esto es el rectangulp
            Collider.Position = new Vector2(r.X, r.Y);
            Collider.Width = r.Width;
            Collider.Height = r.Height;
        }
    }


    private bool HayColisionY()
{
    if (Collider == null) return false;
    foreach (var col in CollisionManager.GetColliders())
    {
        if (col != Collider && Collider.Intersects(col))
            return true;
    }
    return false;
}

    }
    }