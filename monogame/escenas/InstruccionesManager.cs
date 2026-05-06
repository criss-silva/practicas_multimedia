namespace capybara
{
    /// <summary>
    /// Gestor estático que controla si el panel de instrucciones ya fue mostrado
    /// en cada modo de juego. Tiene un flag independiente para el modo secuencial
    /// y para el modo selección de mundos, evitando que se solapen entre sí.
    /// </summary>
    public static class InstruccionesManager
    {
        /// <summary>
        /// Indica si las instrucciones ya se mostraron en el modo secuencial (botón Play).
        /// </summary>
        private static bool _yamostradasSecuencial = false;

        /// <summary>
        /// Indica si las instrucciones ya se mostraron en el modo selección de mundos.
        /// </summary>
        private static bool _yamostradasMundos = false;

        /// <summary>
        /// Devuelve true si las instrucciones ya fueron mostradas en el modo activo actual.
        /// Consulta el flag correcto según <see cref="ModoJuego.EsSeleccionDeMundo"/>.
        /// </summary>
        public static bool YaMostradas =>
            ModoJuego.EsSeleccionDeMundo ? _yamostradasMundos : _yamostradasSecuencial;

        /// <summary>
        /// Marca las instrucciones como ya mostradas para el modo activo actual.
        /// </summary>
        public static void Marcar()
        {
            if (ModoJuego.EsSeleccionDeMundo)
                _yamostradasMundos = true;
            else
                _yamostradasSecuencial = true;
        }

        /// <summary>
        /// Resetea el flag del modo secuencial para que las instrucciones vuelvan
        /// a aparecer en la próxima partida nueva. Solo afecta al modo secuencial.
        /// </summary>
        public static void Reset()
        {
            _yamostradasSecuencial = false;
        }

        /// <summary>
        /// Resetea el flag del modo selección de mundos. Se llama al iniciar
        /// una nueva sesión en ese modo si fuera necesario.
        /// </summary>
        public static void ResetMundos()
        {
            _yamostradasMundos = false;
        }
    }
}