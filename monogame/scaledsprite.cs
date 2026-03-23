using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    internal class ScaledSprite : Sprite
    {
        public float scale;
        
        // Vamos a lidiar aqui con el problema de girar a la izquierda
        public SpriteEffects efecto = SpriteEffects.None; 

        // Variables para ajustar la caja manualmente 
        public int anchoCaja = 50;  // Ancho bb (bounding box)
        public int altoCaja = 50;   // Alto bb
        public int offsetY = -5;    // offset para ajustar bien el suelo(positivo baja hacia abajo)

        
        //Desplazamiento X dependiendo de si es izq o der
        public int offsetXNormal = -10;   // Mueve la caja a la izq/der cuando mira a la derecha
        public int offsetXFlip = 10;   // Mueve la caja a la izq/der cuando hace flip (gira a la izquierda)

        public Rectangle Rect 
        {
            get 
            {
                // Decidimos qué offset X usar según el flip
                int desplazamientoX = (efecto == SpriteEffects.FlipHorizontally) ? offsetXFlip : offsetXNormal;

                return new Rectangle(
                    (int)Math.Round(position.X - (anchoCaja / 2f)) + desplazamientoX,
                    (int)Math.Round(position.Y - (altoCaja / 2f)) + offsetY,
                    anchoCaja,
                    altoCaja
                );
            }
        }

        public ScaledSprite(Texture2D texture, Vector2 position, float scale) 
            : base(texture, position)
        {
            this.scale = scale;
        }
    }
}