using System.IO;
using System.Text.Json;

namespace capybara;

/// <summary>
/// Gestiona el guardado y carga del progreso del jugador en disco local.
/// Usa un archivo JSON simple en el directorio de la aplicación.
/// </summary>
public static class SaveManager
{
    private static string _rutaGuardado = "historial.json";

    public class Historial
    {
        public int Nivel { get; set; } = 1;
        public float PosX { get; set; } = 100;
        public float PosY { get; set; } = 90;
    }

    /// <summary>Devuelve true si existe un archivo de guardado.</summary>
    public static bool ExisteSave() => File.Exists(_rutaGuardado);

    /// <summary>Guarda el nivel y posición del checkpoint actual.</summary>
    public static void Guardar(int nivel, float x, float y)
    {
        Historial data = new Historial { Nivel = nivel, PosX = x, PosY = y };
        string json = JsonSerializer.Serialize(data);
        File.WriteAllText(_rutaGuardado, json);
    }

    /// <summary>Carga el guardado. Devuelve null si no existe.</summary>
    public static Historial Cargar()
    {
        if (!ExisteSave()) return null;
        string json = File.ReadAllText(_rutaGuardado);
        return JsonSerializer.Deserialize<Historial>(json);
    }

    /// <summary>Borra el archivo de guardado al iniciar nueva partida.</summary>
    public static void BorrarSave()
    {
        if (ExisteSave()) File.Delete(_rutaGuardado);
    }
}