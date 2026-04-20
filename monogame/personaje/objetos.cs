using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace capybara;

/// <summary>
/// Entidad enemiga voladora que persigue al jugador una vez que entra en su radio
/// de detección. Hereda de <see cref="ScaledSprite"/> y gestiona su propio sistema
/// de movimiento, animación, colisión con el tilemap y detección de daño.
/// </summary>
/// <remarks>
/// El enemigo opera en dos fases:
/// <list type="number">
///   <item><description>Idle — permanece estático hasta que el jugador entra en <see cref="_detectedRadius"/>.</description></item>
///   <item><description>Chase — persigue al jugador manteniéndose a una altura fija sobre el suelo.</description></item>
/// </list>
/// Opcionalmente puede requerir línea de visión directa para iniciar la persecución
/// activando <see cref="SetRaycastDetection"/>.
/// </remarks>
internal class objetos : ScaledSprite
{
    
    /// <summary>
    /// Posición de spawn original. Se usa para restaurar al enemigo
    /// cuando el jugador pierde una vida o al llamar a <see cref="ResetearPosicion"/>.
    /// </summary>
    private Vector2 _posicionInicial;

    /// <summary>Tamaño de un tile en píxeles. Debe coincidir con el valor usado en las escenas.</summary>
    private int _tileSize = 60;

    /// <summary>Número total de frames de la animación del sprite sheet.</summary>
    private int _totalFrames;

    /// <summary>Índice del frame actualmente visible en el sprite sheet.</summary>
    private int _frameActual;

    /// <summary>Acumulador de tiempo para el avance de la animación.</summary>
    private float _timerAnimacion;

    /// <summary>Duración de cada frame de animación en segundos.</summary>
    private float _tiempoPorFrame = 0.1f;

    /// <summary>Ancho de un frame individual del sprite sheet en píxeles.</summary>
    private int _anchoFrame;

    /// <summary>Alto del sprite sheet en píxeles (equivale al alto de un frame al ser de una sola fila).</summary>
    private int _altoFrame;
 /// <summary>
 /// vector que representa el punto de origen para dibujar el sprite, generalmente el centro del frame
 /// </summary>
    private Vector2 _origen;
   

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Enemigo"/>.
    /// Calcula el tamaño del colisionador en función de la escala del sprite
    /// y lo registra en su posición inicial.
    /// </summary>
    /// <param name="texture">Sprite sheet con todos los frames de animación en una sola fila.</param>
    /// <param name="position">Posición inicial en coordenadas de mundo (también usada como punto de respawn).</param>
    /// <param name="scale">Factor de escala visual aplicado al sprite.</param>
    /// <param name="totalFrames">Número de frames horizontales en el sprite sheet.</param>
    public objetos(Texture2D texture, Vector2 position, float scale, int totalFrames)
        : base(texture, position, scale)
    {
        this.position = position;
        this._posicionInicial = position;
        this._totalFrames = totalFrames;
        this._anchoFrame = texture.Width / _totalFrames;
        this._altoFrame = texture.Height;
        this._origen = new Vector2(_anchoFrame / 2f, _altoFrame / 2f);

        
        
    }

    /// <summary>
    /// Actualiza la lógica del enemigo cada frame: detección del jugador,
    /// movimiento de persecución, animación y comprobación de daño.
    /// Debe invocarse desde el método <c>Update</c> de la escena activa.
    /// </summary>
    /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
    /// <param name="jugador">Referencia al sprite del jugador, usado como objetivo de persecución.</param>
    /// <param name="tilemap">Mapa de tiles de la escena actual, necesario para la navegación y colisiones.</param>
    public void Update(GameTime gameTime, MovedSprite jugador, Dictionary<Vector2, int> tilemap)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timerAnimacion += dt;
        if (_timerAnimacion >= _tiempoPorFrame)
        {
            _frameActual = (_frameActual + 1) % _totalFrames;
            _timerAnimacion = 0;
        }

     
    }

    /// <summary>
    /// Aplica el vector de velocidad actual a la posición del enemigo con resolución
    /// de colisiones contra el tilemap en los ejes X e Y de forma independiente.
    /// Si se detecta colisión en un eje, se revierte el movimiento en ese eje
    /// y se anula la velocidad correspondiente para evitar que el enemigo se quede atascado.
    /// </summary>
    /// <param name="tilemap">Mapa de tiles de la escena actual.</param>
        public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
        spriteBatch.Draw(texture, position, fuente, Color.White, 0f, _origen, scale, efecto, 0f);

       
        
    }

}