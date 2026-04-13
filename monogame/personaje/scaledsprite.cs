using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    /// <summary>
    /// Sprite con escala y soporte para inversión horizontal, que expone un
    /// rectángulo de colisión (bounding box) ajustable de forma independiente
    /// a las dimensiones visuales del sprite.
    /// Sirve de clase base para todas las entidades del juego que necesiten
    /// un colisionador configurable, como el jugador (<see cref="MovedSprite"/>)
    /// y el enemigo (<see cref="Enemigo"/>).
    /// </summary>
    public class ScaledSprite : Sprite
    {
        /// <summary>Factor de escala visual aplicado al sprite al dibujarlo.</summary>
        public float scale;

        /// <summary>
        /// Efecto de volteo horizontal del sprite. Se usa para orientar al personaje
        /// o enemigo según la dirección de movimiento sin necesidad de sprites separados.
        /// Por defecto apunta a la derecha (<see cref="SpriteEffects.None"/>).
        /// </summary>
        public SpriteEffects efecto = SpriteEffects.None;

        /// <summary>
        /// Ancho del bounding box en píxeles. Independiente del ancho visual del sprite,
        /// lo que permite ajustar la hitbox sin modificar el arte.
        /// </summary>
        public int anchoCaja = 50;

        /// <summary>
        /// Alto del bounding box en píxeles. Independiente del alto visual del sprite,
        /// lo que permite ajustar la hitbox sin modificar el arte.
        /// </summary>
        public int altoCaja = 50;

        /// <summary>
        /// Desplazamiento vertical del bounding box respecto al centro del sprite.
        /// Valores positivos mueven la caja hacia abajo; negativos, hacia arriba.
        /// Se usa para alinear la base de la caja con los pies del personaje.
        /// </summary>
        public int offsetY = -5;

        /// <summary>
        /// Desplazamiento horizontal del bounding box cuando el sprite mira a la derecha
        /// (<see cref="SpriteEffects.None"/>). Valores negativos desplazan la caja a la izquierda.
        /// </summary>
        public int offsetXNormal = -10;

        /// <summary>
        /// Desplazamiento horizontal del bounding box cuando el sprite está invertido
        /// (<see cref="SpriteEffects.FlipHorizontally"/>). Valores positivos desplazan
        /// la caja a la derecha para compensar el volteo visual.
        /// </summary>
        public int offsetXFlip = 10;

        /// <summary>
        /// Rectángulo de colisión (bounding box) calculado cada vez que se accede,
        /// centrado en <see cref="Sprite.position"/> y ajustado con los offsets
        /// configurados. El desplazamiento X varía según la dirección actual del sprite
        /// para mantener la hitbox alineada visualmente en ambas orientaciones.
        /// </summary>
        public Rectangle Rect
        {
            get
            {
                int desplazamientoX = (efecto == SpriteEffects.FlipHorizontally) ? offsetXFlip : offsetXNormal;

                return new Rectangle(
                    (int)Math.Round(position.X - (anchoCaja / 2f)) + desplazamientoX,
                    (int)Math.Round(position.Y - (altoCaja / 2f)) + offsetY,
                    anchoCaja,
                    altoCaja
                );
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ScaledSprite"/>.
        /// </summary>
        /// <param name="texture">Textura del sprite.</param>
        /// <param name="position">Posición inicial en coordenadas de mundo.</param>
        /// <param name="scale">Factor de escala visual aplicado al sprite.</param>
        public ScaledSprite(Texture2D texture, Vector2 position, float scale)
            : base(texture, position)
        {
            this.scale = scale;
        }
    }
}