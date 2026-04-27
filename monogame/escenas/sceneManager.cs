using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    /// <summary>
    /// Gestor de escenas basado en una pila (stack). Controla qué escena está
    /// activa en cada momento y permite la navegación hacia adelante (apilar
    /// nuevas escenas) y hacia atrás (desapilar la escena actual).
    /// <para>
    /// La escena en la cima de la pila es la única que recibe las llamadas
    /// de <c>Update</c> y <c>Draw</c> desde <see cref="Game1"/>.
    /// </para>
    /// <para>
    /// Flujo de navegación del juego:
    /// <list type="bullet">
    ///   <item><description>Inicio: <see cref="MenuScene"/> se apila en <see cref="Game1.LoadContent"/>.</description></item>
    ///   <item><description>Al jugar: se apila <see cref="GameScene_M2"/> sobre el menú.</description></item>
    ///   <item><description>Al avanzar de nivel: se apila la siguiente <c>GameScene</c>.</description></item>
    ///   <item><description>Al ganar o perder: se apila <see cref="WinScene"/> o <see cref="GameOverScene"/>.</description></item>
    ///   <item><description>Al volver al menú: se desapilan todas las escenas hasta dejar solo <see cref="MenuScene"/>.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class SceneManager
    {
        /// <summary>
        /// Pila interna de escenas. La escena en la cima (<c>Peek</c>) es la activa.
        /// Se declara como <c>readonly</c> para garantizar que la referencia a la
        /// pila no se reemplaza accidentalmente tras la construcción.
        /// </summary>
        private readonly Stack<IScene> stackescenas;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SceneManager"/> con la pila vacía.
        /// </summary>
        public SceneManager()
        {
            stackescenas = new();
        }

        /// <summary>
        /// Apila una nueva escena sobre la actual, convirtiéndola en la escena activa.
        /// La escena anterior queda suspendida en la pila y se recuperará si la nueva
        /// es desapilada posteriormente.
        /// </summary>
        /// <param name="escena">La escena a apilar. Debe tener su contenido ya cargado.</param>
        public void AddScene(IScene escena)
        {
            stackescenas.Push(escena);
        }

        /// <summary>
        /// Desapila la escena activa, volviendo a la escena inmediatamente inferior
        /// en la pila. Si la pila está vacía no realiza ninguna acción.
        /// </summary>
        public void RemoveScene()
        {
            if (stackescenas.Count > 0)
            {
                stackescenas.Pop();
            }
        }

        /// <summary>
        /// Devuelve la escena activa (la que se encuentra en la cima de la pila)
        /// sin desapilarla.
        /// </summary>
        /// <returns>
        /// La <see cref="IScene"/> activa, o <c>null</c> si la pila está vacía.
        /// </returns>
        public IScene sceneaActual()
        {
            if (stackescenas.Count > 0)
            {
                return stackescenas.Peek();
            }
            return null;
        }
    }
}