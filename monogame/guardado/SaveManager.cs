using System.IO;
using System.Text.Json;

namespace capybara
{
    /// <summary>
    /// Gestor estático del sistema de guardado local del juego.
    /// Usa archivos JSON separados para el modo secuencial y el modo selección
    /// de mundos, evitando que los saves de un modo interfieran con el otro.
    /// </summary>
    public static class SaveManager
    {
        /// <summary>Ruta del archivo de guardado del modo secuencial (botón Play).</summary>
        private static string _rutaSecuencial = "historial.json";

        /// <summary>Ruta del archivo de guardado del modo selección de mundos.</summary>
        private static string _rutaMundos = "historial_mundos.json";

        /// <summary>
        /// Devuelve la ruta correcta según el modo de juego activo.
        /// </summary>
        private static string RutaActual =>
            ModoJuego.EsSeleccionDeMundo ? _rutaMundos : _rutaSecuencial;

        /// <summary>
        /// Modelo de datos que representa el estado guardado del jugador.
        /// Se serializa y deserializa directamente desde el archivo JSON.
        /// </summary>
        public class Historial
        {
            /// <summary>Número del nivel en el que se activó el checkpoint.</summary>
            public int Nivel { get; set; } = 1;

            /// <summary>Coordenada X del jugador en el momento de activar el checkpoint.</summary>
            public float PosX { get; set; } = 100;

            /// <summary>Coordenada Y del jugador en el momento de activar el checkpoint.</summary>
            public float PosY { get; set; } = 90;
        }

        /// <summary>
        /// Comprueba si existe un archivo de guardado para el modo secuencial.
        /// Solo el modo secuencial tiene botón "Continuar", por eso no se consulta
        /// el modo mundos aquí.
        /// </summary>
        /// <returns><c>true</c> si el archivo de guardado secuencial existe.</returns>
        public static bool ExisteSave() => File.Exists(_rutaSecuencial);

        /// <summary>
        /// Serializa y escribe el progreso en el archivo correspondiente al modo activo,
        /// sobreescribiendo cualquier guardado anterior del mismo modo.
        /// </summary>
        /// <param name="nivel">Número del nivel activo en el momento del guardado.</param>
        /// <param name="x">Coordenada X del jugador.</param>
        /// <param name="y">Coordenada Y del jugador.</param>
        public static void Guardar(int nivel, float x, float y)
        {
            // En modo mundos no se guarda progreso para no interferir con el modo secuencial
            if (ModoJuego.EsSeleccionDeMundo) return;

            Historial data = new Historial { Nivel = nivel, PosX = x, PosY = y };
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(_rutaSecuencial, json);
            System.Diagnostics.Debug.WriteLine($"[SaveManager] Guardado nivel {nivel} en: {Path.GetFullPath(_rutaSecuencial)}");
        }

        /// <summary>
        /// Lee y deserializa el archivo de guardado del modo secuencial.
        /// </summary>
        /// <returns>
        /// Instancia de <see cref="Historial"/> con el progreso guardado,
        /// o <c>null</c> si el archivo no existe.
        /// </returns>
        public static Historial Cargar()
        {
            if (!ExisteSave()) return null;
            string json = File.ReadAllText(_rutaSecuencial);
            System.Diagnostics.Debug.WriteLine($"[SaveManager] Cargando save: {json}");
            return JsonSerializer.Deserialize<Historial>(json);
        }

        /// <summary>
        /// Elimina el archivo de guardado del modo secuencial.
        /// Se llama al iniciar una partida nueva o al llegar a game over.
        /// </summary>
        public static void BorrarSave()
        {
            if (ExisteSave()) File.Delete(_rutaSecuencial);
        }
    }
}