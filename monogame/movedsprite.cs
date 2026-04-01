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

        //textura de entrada y salida
        public Texture2D TexEspecial { get; private set; }
        public Texture2D TexSalida { get; private set; } 
        public bool EnEstadoS { get; private set; } = false;
        public bool SaliendoDeS { get; private set; } = false; 
        
        public int FrameActualS { get; private set; } = 0;
        public int TotalFramesS { get; private set; } = 7; 
        public int TotalFramesSalida { get; private set; } = 7; 
        
        private float _timerS = 0f;
        private float _timerAnimacion = 0f;

        public BoxCollider Collider { get; set; }
        private float offsetCompensacionGiro = 20f;

        public MovedSprite(Texture2D texture, Vector2 position, float scale, float speed, Texture2D texEspecial, Texture2D texSalida)
            : base(texture, position, scale)
        {
            this.speed = speed;
            this.TexEspecial = texEspecial;
            this.TexSalida = texSalida;
            this.velocity = Vector2.Zero;
            if (Collider == null)
            {
                Collider = new BoxCollider(position, 70, 70, false);
            }
        }
        public void Update(KeyboardState keyboard, KeyboardState teclaanterior, float gravedad, float fuerza, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

           
            if (keyboard.IsKeyDown(Keys.S) && !EnEstadoS && !SaliendoDeS && teclaanterior.IsKeyUp(Keys.S))
            {
                EnEstadoS = true;
                _timerS = 0f;
                FrameActualS = 0;
            }

            //la logica de la burbuja
            if (EnEstadoS)
            {
                _timerS += dt;
                velocity.X = 0; 

                _timerAnimacion += dt;
                if (_timerAnimacion >= 0.1f)
                {
                    if (FrameActualS < TotalFramesS - 1) FrameActualS++;
                    _timerAnimacion = 0f;
                }

                if (_timerS >= 3f) 
                {
                    EnEstadoS = false;
                    SaliendoDeS = true; 
                    FrameActualS = 0;   
                }

                AplicarFisicasYColisiones(gravedad);
                prevKeyboard = keyboard;
                
            }

            // salida burbuja
            if (SaliendoDeS)
            {
                velocity.X = 0;
                _timerAnimacion += dt;
                if (_timerAnimacion >= 0.088f) 
                {
                    FrameActualS++;
                    _timerAnimacion = 0f;
                }

                if (FrameActualS >= TotalFramesSalida)
                {
                    SaliendoDeS = false;
                }

                AplicarFisicasYColisiones(gravedad);
                prevKeyboard = keyboard;
                
            }

            SpriteEffects efectoAnterior = efecto;

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

            // Compensación de giro
            if (efectoAnterior == SpriteEffects.None && efecto == SpriteEffects.FlipHorizontally)
                position.X -= offsetCompensacionGiro;
            else if (efectoAnterior == SpriteEffects.FlipHorizontally && efecto == SpriteEffects.None)
                position.X += offsetCompensacionGiro;

            escalaX += (escalaObjetivo - escalaX) * velocidadFlip * 0.016f;

            // Salto
            if (keyboard.IsKeyDown(Keys.W) && saltos < 2 && teclaanterior.IsKeyUp(Keys.W))
            {
                saltos++;
                velocity.Y = fuerza;
            }

            AplicarFisicasYColisiones(gravedad);

            prevKeyboard = keyboard;
        }

        
        private void AplicarFisicasYColisiones(float gravedad)
        {
            velocity.Y += gravedad;

            float oldPosY = position.Y;
            position.Y += velocity.Y;
            ActualizarCollider();

            if (HayColisionY())
            {
                position.Y = oldPosY;
                ActualizarCollider();
                if (velocity.Y > 0) saltos = 0;
                velocity.Y = 0;
            }

           
            float oldPosX = position.X;
            position.X += velocity.X;
            ActualizarCollider();

            if (HayColisionY()) 
            {
                position.X = oldPosX;
                ActualizarCollider();
            }
        }

        private void ActualizarCollider()
        {
            if (Collider != null)
            {
                Rectangle r = this.Rect;
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