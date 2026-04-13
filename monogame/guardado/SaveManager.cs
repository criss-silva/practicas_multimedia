using System.IO;
using System.Text.Json;

namespace capybara
{
    /// <summary>
    /// Gestor estático del sistema de guardado local del juego.
    /// Persiste el progreso del jugador en un archivo JSON en el directorio
    /// de la aplicación, almacenando el nivel y la posición del último
    /// checkpoint activado. Al ser estático no requiere instanciación.
    /// </summary>
    public static class SaveManager
    {
        /// <summary>Ruta relativa al archivo JSON de guardado.</summary>
        private static string _rutaGuardado = "historial.json";

        /// <summary>
        /// Modelo de datos que representa el estado guardado del jugador.
        /// Se serializa y deserializa directamente desde el archivo JSON.
        /// </summary>
        public class Historial
        {
            /// <summary>Número del nivel en el que se activó el checkpoint (1, 2 o 3).</summary>
            public int Nivel { get; set; } = 1;

            /// <summary>Coordenada X del jugador en el momento de activar el checkpoint.</summary>
            public float PosX { get; set; } = 100;

            /// <summary>Coordenada Y del jugador en el momento de activar el checkpoint.</summary>
            public float PosY { get; set; } = 90;
        }

        /// <summary>
        /// Comprueba si existe un archivo de guardado en disco.
        /// Se usa en <see cref="EscenaSeleccionada"/> para decidir si mostrar
        /// el botón de continuar activo o desactivado.
        /// </summary>
        /// <returns><c>true</c> si el archivo de guardado existe; <c>false</c> en caso contrario.</returns>
        public static bool ExisteSave() => File.Exists(_rutaGuardado);

        /// <summary>
        /// Serializa y escribe el progreso actual en el archivo JSON de guardado,
        /// sobreescribiendo cualquier guardado anterior. Se invoca automáticamente
        /// desde <see cref="Checkpoint.Update"/> al detectar contacto con el jugador.
        /// </summary>
        /// <param name="nivel">Número del nivel activo en el momento del guardado.</param>
        /// <param name="x">Coordenada X del jugador en el momento del guardado.</param>
        /// <param name="y">Coordenada Y del jugador en el momento del guardado.</param>
        public static void Guardar(int nivel, float x, float y)
        {
            Historial data = new Historial { Nivel = nivel, PosX = x, PosY = y };
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(_rutaGuardado, json);
        }

        /// <summary>
        /// Lee y deserializa el archivo de guardado desde disco.
        /// </summary>
        /// <returns>
        /// Instancia de <see cref="Historial"/> con el progreso guardado,
        /// o <c>null</c> si el archivo no existe.
        /// </returns>
        public static Historial Cargar()
        {
            if (!ExisteSave()) return null;
            string json = File.ReadAllText(_rutaGuardado);
            return JsonSerializer.Deserialize<Historial>(json);
        }

        /// <summary>
        /// Elimina el archivo de guardado del disco.
        /// Debe llamarse al iniciar una partida nueva desde <see cref="EscenaSeleccionada"/>
        /// para que el progreso anterior no interfiera con la nueva sesión.
        /// No hace nada si el archivo no existe.
        /// </summary>
        public static void BorrarSave()
        {
            if (ExisteSave()) File.Delete(_rutaGuardado);
        }
    }
}