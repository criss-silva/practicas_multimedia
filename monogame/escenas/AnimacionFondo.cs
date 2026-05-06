using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    /// <summary>
    /// Gestiona la animación de un fondo mediante un sprite sheet horizontal.
    /// Recorre los frames del sheet a una velocidad configurable en FPS y los
    /// dibuja escalados al rectángulo de destino indicado en cada llamada a <see cref="Draw"/>.
    /// </summary>
    public class AnimacionFondo
    {
        /// <summary>
        /// Sprite sheet que contiene todos los frames de la animación dispuestos
        /// en una sola fila horizontal. El ancho de cada frame se calcula dividiendo
        /// el ancho total entre <see cref="_totalFrames"/>.
        /// </summary>
        private Texture2D _spriteSheet;

        /// <summary>
        /// Número total de frames que contiene el sprite sheet.
        /// Determina tanto el ancho de cada frame como el punto de reinicio del ciclo.
        /// </summary>
        private int _totalFrames;

        /// <summary>
        /// Índice del frame que se está dibujando en el momento actual.
        /// Se incrementa cíclicamente de 0 a <see cref="_totalFrames"/> - 1.
        /// </summary>
        private int _frameActual = 0;

        /// <summary>
        /// Tiempo en segundos que debe permanecer visible cada frame.
        /// Se calcula como 1 / fps en el constructor.
        /// </summary>
        private float _tiempoPorFrame;

        /// <summary>
        /// Acumulador de tiempo transcurrido desde el último cambio de frame.
        /// Cuando supera <see cref="_tiempoPorFrame"/> se avanza al siguiente frame y se resetea.
        /// </summary>
        private float _cronometro = 0f;

        /// <summary>
        /// Ancho en píxeles de un único frame dentro del sprite sheet.
        /// Se calcula una sola vez en el constructor como ancho total / número de frames.
        /// </summary>
        private int _anchoFrame;

        /// <summary>
        /// Alto en píxeles del sprite sheet, equivalente al alto de cada frame
        /// dado que todos los frames están en una sola fila.
        /// </summary>
        private int _altoFrame;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AnimacionFondo"/> con el
        /// sprite sheet y la configuración de animación indicados.
        /// </summary>
        /// <param name="spriteSheet">
        /// Textura con todos los frames del fondo dispuestos horizontalmente.
        /// </param>
        /// <param name="totalFrames">
        /// Número de frames que contiene el sprite sheet.
        /// </param>
        /// <param name="fps">
        /// Velocidad de la animación en frames por segundo. Por defecto 8.
        /// </param>
        public AnimacionFondo(Texture2D spriteSheet, int totalFrames, float fps = 8f)
        {
            _spriteSheet = spriteSheet;
            _totalFrames = totalFrames;
            _tiempoPorFrame = 1f / fps;
            _anchoFrame = spriteSheet.Width / totalFrames;
            _altoFrame = spriteSheet.Height;
        }

        /// <summary>
        /// Actualiza el estado de la animación avanzando al siguiente frame
        /// cuando ha transcurrido el tiempo suficiente. Debe llamarse una vez
        /// por frame desde el método <c>Update</c> de la escena que lo contenga.
        /// </summary>
        /// <param name="gameTime">
        /// Información de tiempo del frame actual proporcionada por MonoGame.
        /// Se usa para acumular el tiempo transcurrido y compararlo con <see cref="_tiempoPorFrame"/>.
        /// </param>
        public void Update(GameTime gameTime)
        {
            _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cronometro >= _tiempoPorFrame)
            {
                _frameActual = (_frameActual + 1) % _totalFrames;
                _cronometro = 0f;
            }
        }

        /// <summary>
        /// Dibuja el frame actual del fondo escalado al rectángulo de destino indicado.
        /// Recorta del sprite sheet únicamente el frame activo mediante un rectángulo fuente.
        /// </summary>
        /// <param name="spriteBatch">
        /// El <see cref="SpriteBatch"/> activo en el que se realiza el dibujado.
        /// </param>
        /// <param name="destino">
        /// Rectángulo de pantalla al que se escala y dibuja el frame actual.
        /// Normalmente cubre toda la pantalla (por ejemplo, <c>new Rectangle(0, 0, 1280, 720)</c>).
        /// </param>
        public void Draw(SpriteBatch spriteBatch, Rectangle destino)
        {
            Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
            spriteBatch.Draw(_spriteSheet, destino, fuente, Color.White);
        }
    }
}