namespace capybara;

public static class VidaManager
{
    public static int Vidas { get; private set; } = 3; // es una clase estática ya que no hace anda solo usaremos sus funciones 

    // Las escenas se suscriben a este evento para saber cuándo respawnear o reiniciar
    public delegate void VidaEvent();
    public static event VidaEvent OnPerderVida;   // queda en la misma fase
    public static event VidaEvent OnGameOver;     // vidas == 0

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

    public static void Resetear()
    {
        Vidas = 3;
    }
}