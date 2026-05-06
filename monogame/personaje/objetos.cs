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

    private bool _activado = false;
    private int _nivel;
    private Rectangle _rectDeteccion;

    public Rectangle RectDeteccion => _rectDeteccion;
    public bool Activado => _activado;

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
    /// guardarProgreso: false en modo selección de mundo → anima pero no guarda.
    /// El guardado se sobreescribe cada vez que el jugador toca el checkpoint,
    /// así siempre se guarda el último checkpoint tocado.
    /// </summary>
    public void Update(GameTime gameTime, MovedSprite jugador, Dictionary<Vector2, int> tilemap, bool guardarProgreso = true)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // FIX a): se elimina !_activado para que guarde cada vez que se toca,
        // no solo la primera. Así el último checkpoint siempre sobreescribe al anterior.
        if (_rectDeteccion.Intersects(jugador.Rect))
        {
            _activado = true;

            // FIX b): solo guarda si el modo lo permite
            if (guardarProgreso)
                SaveManager.Guardar(_nivel, jugador.position.X, jugador.position.Y);
        }

        if (_activado)
        {
            _timerAnimacion += dt;
            if (_timerAnimacion >= _tiempoPorFrame)
            {
                _frameActual = (_frameActual + 1) % _totalFrames;
                _timerAnimacion = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
        spriteBatch.Draw(texture, position, fuente, Color.White, 0f, _origen, scale, efecto, 0f);
    }
}