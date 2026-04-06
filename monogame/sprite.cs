using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace capybara
{
    /// <summary>
    /// Clase base para todas las entidades visuales del juego.
    /// Encapsula los atributos mínimos que cualquier objeto del mundo necesita:
    /// textura, posición y velocidad. Las clases derivadas amplían esta base
    /// añadiendo escala, colisionadores, físicas o lógica de animación.
    /// <para>
    /// Jerarquía de herencia:
    /// <list type="bullet">
    ///   <item><description><see cref="Sprite"/> — base con textura, posición y velocidad.</description></item>
    ///   <item><description><see cref="ScaledSprite"/> — añade escala y bounding box ajustable.</description></item>
    ///   <item><description><see cref="MovedSprite"/> — añade físicas, entrada del jugador y escudo.</description></item>
    ///   <item><description><see cref="Enemigo"/> — añade IA de persecución y detección de daño.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class Sprite
    {
        /// <summary>
        /// Textura principal del sprite. Contiene el fotograma base o el sprite sheet
        /// completo según el tipo de entidad.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// Posición del sprite en coordenadas de mundo, expresada en píxeles.
        /// Representa el centro visual del sprite en todas las clases derivadas.
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// Vector de velocidad del sprite en píxeles por frame.
        /// En la clase base se inicializa a <see cref="Vector2.Zero"/> y su uso
        /// concreto queda a cargo de las clases derivadas.
        /// </summary>
        public Vector2 velocity;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="Sprite"/> con la textura
        /// y posición indicadas. La velocidad inicial es <see cref="Vector2.Zero"/>.
        /// </summary>
        /// <param name="texture">Textura del sprite.</param>
        /// <param name="position">Posición inicial en coordenadas de mundo.</param>
        public Sprite(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
            this.velocity = Vector2.Zero;
        }

        /// <summary>
        /// Método de actualización virtual. La implementación base está vacía;
        /// las clases derivadas lo sobreescriben para añadir lógica de movimiento,
        /// animación o IA específica de cada entidad.
        /// </summary>
        public virtual void Update() { }
    }
}