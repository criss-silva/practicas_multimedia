using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace capybara
{
    /// <summary>
    /// Representa un colisionador rectangular (AABB - Axis-Aligned Bounding Box).
    /// Gestiona la detección de colisiones entre entidades del juego y expone
    /// un sistema de eventos inspirado en Unity (Enter / Stay / Exit) que permite
    /// a los suscriptores reaccionar ante cada fase de una colisión sin necesidad
    /// de consultar el estado manualmente cada frame.
    /// </summary>
    public class BoxCollider
    {
        /// <summary>
        /// Delegado base para todos los eventos de colisión.
        /// Cualquier método que acepte un <see cref="BoxCollider"/> como parámetro
        /// puede suscribirse a los eventos de esta clase.
        /// </summary>
        /// <param name="other">El otro colisionador implicado en la colisión.</param>
        public delegate void CollisionEvent(BoxCollider other);

        /// <summary>
        /// Se dispara una única vez en el frame en que este colisionador
        /// comienza a solaparse con <paramref name="other"/> por primera vez.
        /// </summary>
        public event CollisionEvent OnCollisionEnter;

        /// <summary>
        /// Se dispara cada frame mientras este colisionador permanece
        /// en contacto con <paramref name="other"/>.
        /// </summary>
        public event CollisionEvent OnCollisionStay;

        /// <summary>
        /// Se dispara una única vez en el frame en que este colisionador
        /// deja de estar en contacto con <paramref name="other"/>.
        /// </summary>
        public event CollisionEvent OnCollisionExit;

        /// <summary>
        /// Indica si este colisionador actúa como trigger.
        /// Un trigger detecta solapamientos y dispara eventos, pero no
        /// participa en la resolución física de colisiones.
        /// </summary>
        public bool IsTrigger;

        /// <summary>
        /// Habilita o deshabilita este colisionador.
        /// Cuando es <c>false</c>, el colisionador es ignorado por completo
        /// en todas las comprobaciones de intersección.
        /// </summary>
        public bool IsActive = true;

        /// <summary>Posición de la esquina superior izquierda del colisionador en píxeles.</summary>
        public Vector2 Position;

        /// <summary>Ancho del colisionador en píxeles.</summary>
        public int Width;

        /// <summary>Alto del colisionador en píxeles.</summary>
        public int Height;

        /// <summary>
        /// Lista interna de colisionadores con los que este objeto está actualmente
        /// en contacto. Se utiliza para determinar las transiciones Enter / Stay / Exit.
        /// </summary>
        private List<BoxCollider> currentCollisions = new List<BoxCollider>();

        /// <summary>Coordenada Y del borde superior del colisionador.</summary>
        public int Top { get { return (int)Position.Y; } }

        /// <summary>Coordenada Y del borde inferior del colisionador.</summary>
        public int Bottom { get { return (int)Position.Y + Height; } }

        /// <summary>Coordenada X del borde izquierdo del colisionador.</summary>
        public int Left { get { return (int)Position.X; } }

        /// <summary>Coordenada X del borde derecho del colisionador.</summary>
        public int Right { get { return (int)Position.X + Width; } }

        /// <summary>
        /// Punto central del colisionador en coordenadas de mundo.
        /// El setter reposiciona el colisionador de forma que su centro
        /// quede en el valor asignado.
        /// </summary>
        public Vector2 Center
        {
            get { return Position + new Vector2(Width / 2, Height / 2); }
            set { Position = new Vector2(value.X - Width / 2, value.Y - Height / 2); }
        }

        /// <summary>
        /// Etiqueta de propiedad utilizada por <see cref="CollisionManager"/> para
        /// identificar a qué entidad pertenece este colisionador (p. ej. "player", "enemy").
        /// Permite filtrar pares de colisión específicos, como ignorar la colisión
        /// jugador-enemigo durante el escudo.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="BoxCollider"/>.
        /// </summary>
        /// <param name="position">Posición inicial de la esquina superior izquierda.</param>
        /// <param name="width">Ancho del colisionador en píxeles.</param>
        /// <param name="height">Alto del colisionador en píxeles.</param>
        /// <param name="isTrigger">
        /// Si es <c>true</c>, el colisionador solo detecta solapamientos sin
        /// intervenir en la física. Por defecto es <c>false</c>.
        /// </param>
        public BoxCollider(Vector2 position, int width, int height, bool isTrigger = false)
        {
            Position = position;
            Width = width;
            Height = height;
            IsTrigger = isTrigger;
        }

        /// <summary>
        /// Comprueba si este colisionador se solapa con <paramref name="other"/>
        /// usando el algoritmo AABB estándar.
        /// Devuelve <c>false</c> automáticamente si cualquiera de los dos
        /// colisionadores está inactivo (<see cref="IsActive"/> = <c>false</c>).
        /// </summary>
        /// <param name="other">El colisionador contra el que se comprueba la intersección.</param>
        /// <returns><c>true</c> si ambos rectángulos se solapan; <c>false</c> en caso contrario.</returns>
        public bool Intersects(BoxCollider other)
        {
            if (!IsActive || !other.IsActive)
                return false;

            return !(Right < other.Left || other.Right < Left ||
                     Bottom < other.Top || other.Bottom < Top);
        }

        /// <summary>
        /// Determina si este colisionador está impactando contra la cara superior
        /// de <paramref name="other"/>, usando una comparación vectorial de áreas.
        /// Útil para detectar si el jugador aterriza sobre una plataforma.
        /// </summary>
        /// <param name="other">El colisionador de referencia.</param>
        /// <returns><c>true</c> si el impacto proviene de arriba.</returns>
        public bool CollidesWithTopOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy <= -hx && wy <= hx;
        }

        /// <summary>
        /// Determina si este colisionador está impactando contra la cara inferior
        /// de <paramref name="other"/>.
        /// Útil para detectar golpes de cabeza contra el techo.
        /// </summary>
        /// <param name="other">El colisionador de referencia.</param>
        /// <returns><c>true</c> si el impacto proviene de abajo.</returns>
        public bool CollidesWithBottomOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy > -hx && wy > hx;
        }

        /// <summary>
        /// Determina si este colisionador está impactando contra la cara izquierda
        /// de <paramref name="other"/>.
        /// </summary>
        /// <param name="other">El colisionador de referencia.</param>
        /// <returns><c>true</c> si el impacto proviene de la izquierda.</returns>
        public bool CollidesWithLeftOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy <= -hx && wy > hx;
        }

        /// <summary>
        /// Determina si este colisionador está impactando contra la cara derecha
        /// de <paramref name="other"/>.
        /// </summary>
        /// <param name="other">El colisionador de referencia.</param>
        /// <returns><c>true</c> si el impacto proviene de la derecha.</returns>
        public bool CollidesWithRightOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy > -hx && wy <= hx;
        }

        /// <summary>
        /// Evalúa el estado de colisión entre este colisionador y <paramref name="other"/>
        /// y dispara el evento correspondiente según la transición de estado:
        /// <list type="bullet">
        ///   <item><description><see cref="OnCollisionEnter"/> — primer frame de contacto.</description></item>
        ///   <item><description><see cref="OnCollisionStay"/> — frames sucesivos en contacto.</description></item>
        ///   <item><description><see cref="OnCollisionExit"/> — primer frame sin contacto tras haberlo tenido.</description></item>
        /// </list>
        /// Debe llamarse cada frame desde <see cref="CollisionManager"/>.
        /// </summary>
        /// <param name="other">El colisionador contra el que se evalúa el estado.</param>
        public void CheckCollision(BoxCollider other)
        {
            if (!Intersects(other))
            {
                if (currentCollisions.Contains(other))
                {
                    if (OnCollisionExit != null)
                        OnCollisionExit(other);
                    currentCollisions.Remove(other);
                }
                return;
            }

            if (currentCollisions.Contains(other))
            {
                if (OnCollisionStay != null)
                    OnCollisionStay(other);
            }
            else
            {
                currentCollisions.Add(other);
                if (OnCollisionEnter != null)
                    OnCollisionEnter(other);
            }
        }

        /// <summary>
        /// Elimina todos los registros de colisiones activas.
        /// Debe llamarse al desactivar o destruir el colisionador para evitar
        /// que eventos obsoletos se disparen en frames posteriores.
        /// </summary>
        public void ClearCollisions()
        {
            currentCollisions.Clear();
        }
    }
}
