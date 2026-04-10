using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace capybara;

/// <summary>
/// Heads-Up Display del juego. Muestra en pantalla el número de vidas
/// restantes del jugador mediante sprites específicos para cada estado.
/// Se dibuja por encima de la escena activa y solo es visible durante
/// las escenas de juego (<see cref="GameScene"/>, <see cref="GameScene2"/>,
/// <see cref="GameScene3"/>), controlado desde <see cref="Game1"/>.
/// </summary>
public class HUD
{
    /// <summary>Sprite que representa el estado de 3 vidas.</summary>
    private Texture2D _vidas3;

    /// <summary>Sprite que representa el estado de 2 vidas.</summary>
    private Texture2D _vidas2;

    /// <summary>Sprite que representa el estado de 1 vida.</summary>
    private Texture2D _vidas1;

    /// <summary>
    /// Rectángulo de destino en pantalla donde se dibuja el sprite de vidas.
    /// Se posiciona en la esquina superior derecha y se calcula definitivamente
    /// en <see cref="LoadContent"/> una vez conocidas las dimensiones reales del sprite.
    /// </summary>
    private Rectangle _rectVidas;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HUD"/> con un rectángulo
    /// provisional fuera de pantalla. El rectángulo definitivo se establece
    /// en <see cref="LoadContent"/> tras cargar las texturas.
    /// </summary>
    public HUD()
    {
        _rectVidas = new Rectangle(1280 - 160, -200, 160, 60);
    }

    /// <summary>
    /// Carga las tres texturas de vidas y calcula el rectángulo de destino
    /// escalando el sprite original para que sea visible en pantalla.
    /// El HUD se ancla a la esquina superior derecha con un margen de 10 píxeles.
    /// </summary>
    /// <param name="content">Gestor de contenido de MonoGame para la carga de assets.</param>
    public void LoadContent(ContentManager content)
    {
        _vidas3 = content.Load<Texture2D>("vidas");
        _vidas2 = content.Load<Texture2D>("2vidas");
        _vidas1 = content.Load<Texture2D>("1vida");

        // Las imágenes originales son muy pequeñas; se aplica un factor de escala
        // para que sean legibles en la resolución de juego (1280×720).
        float escala = 3.0f;
        int ancho = (int)(_vidas3.Width * escala);
        int alto = (int)(_vidas3.Height * escala);
        _rectVidas = new Rectangle(1280 - ancho - 10, -50, ancho, alto);
    }

    /// <summary>
    /// Dibuja el sprite de vidas correspondiente al estado actual del
    /// <see cref="VidaManager"/>. Si el jugador no tiene vidas restantes
    /// (estado de game over en tránsito) no se dibuja nada.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (VidaManager.Vidas == 0) return;

        Texture2D spriteVidas = VidaManager.Vidas switch
        {
            3 => _vidas3,
            2 => _vidas1,
            1 => _vidas2,
            _ => _vidas2
        };

        spriteBatch.Draw(spriteVidas, _rectVidas, Color.White);
    }
}