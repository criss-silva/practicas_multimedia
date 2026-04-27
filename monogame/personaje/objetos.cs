using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace capybara;

/// <summary>
/// Objeto de escena que actúa como checkpoint animado. Permanece en su frame
/// inicial hasta que el jugador lo toca, momento en el que activa la animación
/// en bucle y persiste el progreso en <see cref="SaveManager"/>.
/// Hereda de <see cref="ScaledSprite"/>.
/// </summary>
internal class objetos : ScaledSprite
{
    private Vector2 _posicionInicial;
    private int _tileSize = 60;
    private int _totalFrames;
    private int _frameActual;
    private float _timerAnimacion;
    private float _tiempoPorFrame = 0.1f;
    private int _anchoFrame;
    private int _altoFrame;
    private Vector2 _origen;

    /// <summary>Indica si el jugador ya ha tocado este objeto.</summary>
    private bool _activado = false;

    /// <summary>Número del nivel al que pertenece este objeto, usado al guardar el progreso.</summary>
    private int _nivel;

    /// <summary>Rectángulo de detección de contacto con el jugador.</summary>
    private Rectangle _rectDeteccion;

    /// <summary>Expone el rectángulo de detección para depuración visual.</summary>
    public Rectangle RectDeteccion => _rectDeteccion;

    /// <summary>Indica si el objeto ha sido activado por el jugador.</summary>
    public bool Activado => _activado;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="objetos"/>.
    /// </summary>
    /// <param name="texture">Sprite sheet horizontal con todos los frames de animación.</param>
    /// <param name="position">Posición inicial en coordenadas de mundo.</param>
    /// <param name="scale">Factor de escala visual.</param>
    /// <param name="totalFrames">Número de frames horizontales en el sprite sheet.</param>
    /// <param name="nivel">Número del nivel activo, almacenado en el guardado al activarse.</param>
    public objetos(Texture2D texture, Vector2 position, float scale, int totalFrames, int nivel = 1)
        : base(texture, position, scale)
    {
        this.position = position;
        this._posicionInicial = position;
        this._totalFrames = totalFrames;
        this._anchoFrame = texture.Width / _totalFrames;
        this._altoFrame = texture.Height;
        this._origen = new Vector2(_anchoFrame / 2f, _altoFrame / 2f);
        this._nivel = nivel;

        // Rectángulo de detección basado en el tamaño escalado del primer frame
        float factorDeteccion = 0.4f;
        int anchoDeteccion = (int)(_anchoFrame * scale * factorDeteccion);
        int altoDeteccion = (int)(_altoFrame * scale * factorDeteccion);
        _rectDeteccion = new Rectangle(
            (int)(position.X - anchoDeteccion / 2f),
            (int)(position.Y - altoDeteccion / 2f),
            anchoDeteccion,
            altoDeteccion);
    }

    /// <summary>
    /// Actualiza la animación y comprueba el contacto con el jugador.
    /// La animación solo avanza tras la activación. Al activarse guarda
    /// el progreso en <see cref="SaveManager"/>.
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual.</param>
    /// <param name="jugador">Referencia al sprite del jugador.</param>
    /// <param name="tilemap">Mapa de tiles de la escena actual.</param>
    public void Update(GameTime gameTime, MovedSprite jugador, Dictionary<Vector2, int> tilemap)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Detectar contacto y activar
        if (!_activado && _rectDeteccion.Intersects(jugador.Rect))
            {
                _activado = true;
                SaveManager.Guardar(_nivel, jugador.position.X, jugador.position.Y);
            }

        // La animación solo avanza si está activado
        if (_activado)
        {
            _timerAnimacion += dt;
            if (_timerAnimacion >= _tiempoPorFrame)
            {
                _frameActual = (_frameActual + 1) % _totalFrames;
                _timerAnimacion = 0;
            }
        }
        // Si no está activado se queda en frame 0
    }

    /// <summary>
    /// Dibuja el frame actual del sprite sheet en pantalla.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
        spriteBatch.Draw(texture, position, fuente, Color.White, 0f, _origen, scale, efecto, 0f);
    }
}