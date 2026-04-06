using System.Collections.Generic;

namespace capybara
{
    /// <summary>
    /// Gestor global y estático del sistema de colisiones.
    /// Centraliza el registro, la actualización y la eliminación de todos los
    /// <see cref="BoxCollider"/> activos en la escena actual.
    /// Al ser estático no requiere instanciación; todas sus operaciones se
    /// invocan directamente sobre la clase.
    /// </summary>
    public static class CollisionManager
    {
        /// <summary>
        /// Cuando es <c>true</c>, las colisiones entre colisionadores con
        /// <see cref="BoxCollider.Owner"/> "player" y "enemy" son ignoradas
        /// completamente. Se activa durante el escudo del jugador (estado S)
        /// y se desactiva al finalizar dicho estado.
        /// </summary>
        public static bool IgnorePlayerEnemyCollisions { get; set; } = false;

        /// <summary>
        /// Lista maestra de todos los colisionadores registrados en la escena actual.
        /// </summary>
        private static List<BoxCollider> colliders = new List<BoxCollider>();

        /// <summary>
        /// Lista de colisionadores pendientes de eliminación al final del frame.
        /// Se utiliza una lista separada para evitar modificar <see cref="colliders"/>
        /// mientras se itera sobre ella durante <see cref="CheckCollisions"/>.
        /// </summary>
        private static List<BoxCollider> toRemove = new List<BoxCollider>();

        /// <summary>
        /// Registra un nuevo <see cref="BoxCollider"/> en el sistema de colisiones.
        /// A partir de este momento el colisionador participará en las comprobaciones
        /// de cada frame.
        /// </summary>
        /// <param name="collider">El colisionador a registrar.</param>
        public static void AddCollider(BoxCollider collider)
        {
            colliders.Add(collider);
        }

        /// <summary>
        /// Marca un <see cref="BoxCollider"/> para su eliminación al final del
        /// próximo frame. No se elimina de forma inmediata para evitar
        /// modificar la colección mientras se itera en <see cref="CheckCollisions"/>.
        /// </summary>
        /// <param name="collider">El colisionador a eliminar.</param>
        public static void RemoveCollider(BoxCollider collider)
        {
            toRemove.Add(collider);
        }

        /// <summary>
        /// Marca todos los colisionadores registrados para su eliminación.
        /// Debe llamarse al cambiar de escena para garantizar que no queden
        /// referencias obsoletas que contaminen la escena entrante.
        /// </summary>
        public static void Clear()
        {
            toRemove.AddRange(colliders);
        }

        /// <summary>
        /// Punto de entrada del ciclo de vida del gestor. Debe invocarse una vez
        /// por frame desde la escena activa.
        /// Ejecuta en orden:
        /// <list type="number">
        ///   <item><description>Comprobación de colisiones entre todos los pares activos.</description></item>
        ///   <item><description>Eliminación de los colisionadores pendientes en <see cref="toRemove"/>.</description></item>
        /// </list>
        /// </summary>
        public static void Update()
        {
            CheckCollisions();

            foreach (var collider in toRemove)
            {
                colliders.Remove(collider);
                collider.ClearCollisions();
            }
            toRemove.Clear();
        }

        /// <summary>
        /// Recorre todos los pares posibles de colisionadores activos y delega
        /// la evaluación del estado de cada par en <see cref="BoxCollider.CheckCollision"/>.
        /// <para>
        /// Aplica los siguientes filtros antes de evaluar cada par:
        /// <list type="bullet">
        ///   <item><description>Descarta el par si ambos elementos son el mismo objeto.</description></item>
        ///   <item><description>Descarta el par si alguno de los dos está inactivo.</description></item>
        ///   <item><description>Descarta el par jugador-enemigo si <see cref="IgnorePlayerEnemyCollisions"/> es <c>true</c>.</description></item>
        /// </list>
        /// </para>
        /// <remarks>
        /// El algoritmo actual es O(n²). Para el volumen de colisionadores
        /// previsto en el proyecto es suficiente; si el número de entidades
        /// crece significativamente considerar una estructura espacial (quadtree, grid).
        /// </remarks>
        /// </summary>
        private static void CheckCollisions()
        {
            foreach (var colliderA in colliders)
            {
                foreach (var colliderB in colliders)
                {
                    if (colliderA != colliderB && colliderA.IsActive && colliderB.IsActive)
                    {
                        if (IgnorePlayerEnemyCollisions &&
                            ((colliderA.Owner == "player" && colliderB.Owner == "enemy") ||
                             (colliderA.Owner == "enemy" && colliderB.Owner == "player")))
                        {
                            continue;
                        }
                        colliderA.CheckCollision(colliderB);
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve la lista interna de colisionadores registrados.
        /// Utilizado principalmente por <see cref="MovedSprite"/> para comprobar
        /// colisiones físicas durante el movimiento del jugador sin pasar por
        /// el sistema de eventos.
        /// </summary>
        /// <returns>
        /// Referencia directa a la lista interna de <see cref="BoxCollider"/>.
        /// No modificar externamente; usar <see cref="AddCollider"/> y
        /// <see cref="RemoveCollider"/> para gestionar el ciclo de vida.
        /// </returns>
        public static List<BoxCollider> GetColliders()
        {
            return colliders;
        }
    }
}