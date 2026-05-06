using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    /// <summary>
    /// Objeto de juego que actúa como punto de guardado al ser tocado por el jugador.
    /// Antes de activarse muestra una imagen estática; al activarse reproduce una
    /// animación en bucle extraída de un sprite sheet horizontal y persiste el
    /// progreso en <see cref="SaveManager"/>.
    /// </summary>
    public class Checkpoint
    {
        /// <summary>Textura estática mostrada antes de que el jugador active el checkpoint.</summary>
        private Texture2D _texturaEstatica;

        /// <summary>
        /// Sprite sheet horizontal con los frames de la animación de activación.
        /// Todos los frames tienen el mismo ancho y ocupan una única fila.
        /// </summary>
        private Texture2D _texturaAnimacion;

        /// <summary>Posición en coordenadas de mundo donde se dibuja el checkpoint.</summary>
        private Vector2 _posicion;

        /// <summary>
        /// Rectángulo de detección de contacto con el jugador.
        /// Se calcula a partir de la posición y el tamaño escalado de la textura estática.
        /// </summary>
        private Rectangle _rect;

        /// <summary>Rectángulo de detección usado para depuración visual.</summary>
        public Rectangle Rect => _rect;

        /// <summary>Indica si el jugador ya ha tocado este checkpoint.</summary>
        private bool _activado = false;

        /// <summary>Número del nivel al que pertenece este checkpoint. Se pasa a <see cref="SaveManager.Guardar"/>.</summary>
        private int _nivel;

        /// <summary>Índice del frame actualmente visible en el sprite sheet de animación.</summary>
        private int _frameActual = 0;

        /// <summary>Número total de frames horizontales en el sprite sheet de animación.</summary>
        private int _totalFrames;

        /// <summary>Acumulador de tiempo para el avance de la animación.</summary>
        private float _timerAnimacion = 0f;

        /// <summary>Duración de cada frame de animación en segundos.</summary>
        private float _tiempoPorFrame = 0.15f;

        /// <summary>Ancho en píxeles de un único frame dentro del sprite sheet.</summary>
        private int _anchoFrame;

        /// <summary>
        /// Factor de escala aplicado al dibujo de ambas texturas.
        /// Permite hacer el checkpoint más grande sin modificar los assets originales.
        /// </summary>
        private float _escala;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="Checkpoint"/>.
        /// </summary>
        /// <param name="texturaEstatica">Textura mostrada mientras el checkpoint no está activado.</param>
        /// <param name="texturaAnimacion">Sprite sheet horizontal reproducido al activarse.</param>
        /// <param name="posicion">Posición en coordenadas de mundo.</param>
        /// <param name="nivel">Número del nivel activo, almacenado en el guardado.</param>
        /// <param name="totalFrames">Número de frames horizontales del sprite sheet de animación.</param>
        /// <param name="escala">Factor de escala visual. Por defecto 2.0 para que sea bien visible.</param>
        public Checkpoint(Texture2D texturaEstatica, Texture2D texturaAnimacion,
                          Vector2 posicion, int nivel, int totalFrames, float escala = 2.0f)
        {
            _texturaEstatica = texturaEstatica;
            _texturaAnimacion = texturaAnimacion;
            _posicion = posicion;
            _nivel = nivel;
            _totalFrames = totalFrames;
            _escala = escala;
            _anchoFrame = texturaAnimacion.Width / totalFrames;

            // El rect de detección usa el tamaño escalado de la textura estática
            _rect = new Rectangle(
                (int)posicion.X,
                (int)posicion.Y,
                (int)(_texturaEstatica.Width * escala),
                (int)(_texturaEstatica.Height * escala));
        }

        /// <summary>
        /// Comprueba cada frame si el jugador toca el checkpoint y, en caso afirmativo,
        /// lo activa y guarda el progreso. Una vez activado solo avanza la animación,
        /// nunca vuelve al estado inactivo.
        /// </summary>
        /// <param name="gameTime">Información de tiempo del frame actual.</param>
        /// <param name="jugador">Referencia al sprite del jugador para la detección de contacto.</param>
        public void Update(GameTime gameTime, MovedSprite jugador)
        {
            if (!_activado && _rect.Intersects(jugador.Rect))
            {
                _activado = true;
                // Guardamos la posición del jugador (no la del checkpoint) con un pequeño
                // offset hacia arriba para que el respawn nunca quede dentro de un tile.
                const float offsetRespawnY = -5f;
                SaveManager.Guardar(_nivel, jugador.position.X, jugador.position.Y + offsetRespawnY);
            }

            // La animación solo avanza tras la activación
            if (_activado)
            {
                _timerAnimacion += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timerAnimacion >= _tiempoPorFrame)
                {
                    _frameActual = (_frameActual + 1) % _totalFrames;
                    _timerAnimacion = 0f;
                }
            }
        }

        /// <summary>
        /// Dibuja el checkpoint en pantalla. Antes de activarse muestra la textura
        /// estática escalada; después reproduce la animación frame a frame recortando
        /// el sprite sheet con un rectángulo fuente del tamaño exacto de un frame,
        /// evitando el efecto de cinta de cine que aparece al no especificar el
        /// rectángulo destino con las dimensiones correctas.
        /// </summary>
        /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_activado)
            {
                // Rectángulo destino escalado para la imagen estática
                Rectangle destEstatica = new Rectangle(
                    (int)_posicion.X,
                    (int)_posicion.Y,
                    (int)(_texturaEstatica.Width * _escala),
                    (int)(_texturaEstatica.Height * _escala));

                spriteBatch.Draw(_texturaEstatica, destEstatica, Color.White);
            }
            else
            {
                // Rectángulo fuente: recorta exactamente un frame del spritesheet
                Rectangle fuente = new Rectangle(
                    _frameActual * _anchoFrame, 0,
                    _anchoFrame, _texturaAnimacion.Height);

                // Rectángulo destino: mismo tamaño que la estática para que no cambie el tamaño al activarse
                Rectangle destAnimacion = new Rectangle(
                    (int)_posicion.X,
                    (int)_posicion.Y,
                    (int)(_anchoFrame * _escala),
                    (int)(_texturaAnimacion.Height * _escala));

                spriteBatch.Draw(_texturaAnimacion, destAnimacion, fuente, Color.White);
            }
        }

        /// <summary>Indica si el checkpoint ha sido activado por el jugador.</summary>
        public bool Activado => _activado;
    }
}