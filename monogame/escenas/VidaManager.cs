namespace capybara;

/// <summary>
/// Gestor global y estático del sistema de vidas del jugador.
/// Centraliza el estado de las vidas y notifica a las escenas suscritas
/// mediante eventos cuando el jugador pierde una vida o agota todas ellas.
/// Al ser estático no requiere instanciación; todas sus operaciones se
/// invocan directamente sobre la clase.
/// <para>
/// Las escenas de juego (<see cref="GameScene_M2"/>, <see cref="GameScene2_M2"/>,
/// <see cref="GameScene3_M2"/>) se suscriben a <see cref="OnPerderVida"/> y
/// <see cref="OnGameOver"/> en su constructor y se desuscriben antes de
/// abandonar la escena para evitar referencias colgadas.
/// </para>
/// </summary>
public static class VidaManager
{
    /// <summary>
    /// Número de vidas restantes del jugador. Se inicializa a 3 al arrancar
    /// la aplicación y se resetea a 3 cada vez que se llama a <see cref="Resetear"/>.
    /// Solo puede modificarse internamente a través de <see cref="PerderVida"/>
    /// y <see cref="Resetear"/>.
    /// </summary>
    public static int Vidas { get; private set; } = 3;

    /// <summary>
    /// Delegado base para los eventos de vida. Cualquier método sin parámetros
    /// ni valor de retorno puede suscribirse a los eventos de esta clase.
    /// </summary>
    public delegate void VidaEvent();

    /// <summary>
    /// Se dispara cuando el jugador pierde una vida pero aún le quedan vidas
    /// restantes (<see cref="Vidas"/> mayor que 0). Las escenas suscritas
    /// deben reposicionar al jugador en el punto de spawn del nivel actual.
    /// </summary>
    public static event VidaEvent OnPerderVida;

    /// <summary>
    /// Se dispara cuando el jugador pierde su última vida (<see cref="Vidas"/>
    /// llega a 0). Las escenas suscritas deben limpiar el estado del nivel y
    /// cargar la pantalla de <see cref="GameOverScene"/>.
    /// </summary>
    public static event VidaEvent OnGameOver;

    /// <summary>
    /// Resta una vida al jugador y dispara el evento correspondiente según
    /// el estado resultante:
    /// <list type="bullet">
    ///   <item><description>Si quedan vidas, dispara <see cref="OnPerderVida"/> para que la escena active el respawn.</description></item>
    ///   <item><description>Si las vidas llegan a 0, fija el valor en 0 y dispara <see cref="OnGameOver"/> para transicionar a la pantalla de derrota.</description></item>
    /// </list>
    /// </summary>
    public static void PerderVida()
    {
        Vidas--;
        if (Vidas <= 0)
        {
            Vidas = 0;
            OnGameOver?.Invoke();
        }
        else
        {
            OnPerderVida?.Invoke();
        }
    }

    /// <summary>
    /// Restaura las vidas del jugador a su valor inicial (3).
    /// Debe llamarse al reiniciar la partida desde <see cref="GameOverScene"/>
    /// o al volver al menú desde <see cref="WinScene"/> o <see cref="GameOverScene"/>.
    /// No dispara ningún evento.
    /// </summary>
    public static void Resetear()
    {
        Vidas = 3;
    }
}