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
internal class Enemigo : ScaledSprite
{
    /// <summary>Velocidad de desplazamiento horizontal en píxeles por frame.</summary>
    private float _speed = 1.3f;

    /// <summary>
    /// Radio en píxeles dentro del cual el jugador activa el estado de persecución.
    /// Una vez activado, el enemigo no vuelve al estado idle aunque el jugador salga del radio.
    /// </summary>
    private float _detectedRadius = 300f;

    /// <summary>
    /// Indica si el jugador ya ha entrado al menos una vez en el radio de detección.
    /// Se mantiene en <c>true</c> de forma permanente para evitar que el enemigo
    /// deje de perseguir al salir del radio.
    /// </summary>
    private bool _hasEnteredDeteccion = false;

    /// <summary>
    /// Si es <c>true</c>, además del radio se exige línea de visión directa
    /// (sin tiles sólidos entre el enemigo y el jugador) para iniciar la persecución.
    /// </summary>
    private bool _useRaycastDetection = false;

    /// <summary>Activa la salida de información de depuración en consola y pantalla.</summary>
    private bool _debugMode = false;

    /// <summary>
    /// Desplazamiento manual del colisionador respecto al centro del sprite.
    /// Permite ajustar la hitbox sin modificar la posición visual del enemigo.
    /// </summary>
    private Vector2 _colliderOffset = Vector2.Zero;

    /// <summary>
    /// Factor de escala aplicado al lado del colisionador cuadrado respecto
    /// al tamaño del frame en mundo (0.0–1.0). Valor recomendado: 0.25.
    /// </summary>
    private float _colliderSizeFactor = 0.25f;

    /// <summary>Vector de velocidad actual del enemigo en píxeles por frame.</summary>
    private Vector2 _velocity;

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
    /// Colisionador AABB del enemigo. Cuadrado y centrado sobre el sprite.
    /// Su tamaño se calcula a partir de <see cref="_colliderSizeFactor"/> en el constructor.
    /// </summary>
    public BoxCollider Collider { get; private set; }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Enemigo"/>.
    /// Calcula el tamaño del colisionador en función de la escala del sprite
    /// y lo registra en su posición inicial.
    /// </summary>
    /// <param name="texture">Sprite sheet con todos los frames de animación en una sola fila.</param>
    /// <param name="position">Posición inicial en coordenadas de mundo (también usada como punto de respawn).</param>
    /// <param name="scale">Factor de escala visual aplicado al sprite.</param>
    /// <param name="totalFrames">Número de frames horizontales en el sprite sheet.</param>
    public Enemigo(Texture2D texture, Vector2 position, float scale, int totalFrames)
        : base(texture, position, scale)
    {
        this.position = position;
        this._posicionInicial = position;
        this._totalFrames = totalFrames;
        this._anchoFrame = texture.Width / _totalFrames;
        this._altoFrame = texture.Height;

        int anchoFrameWorld = (int)(_anchoFrame * scale);
        int altoFrameWorld = (int)(_altoFrame * scale);
        int side = (int)(Math.Min(anchoFrameWorld, altoFrameWorld) * _colliderSizeFactor);
        if (side < 4) side = 4;
        Collider = new BoxCollider(position, side, side, false);
        UpdateColliderSize(_colliderSizeFactor);
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

        float dist = Vector2.Distance(this.position, jugador.position);
        if (!_hasEnteredDeteccion && dist <= _detectedRadius)
        {
            _hasEnteredDeteccion = true;
        }

        bool shouldChase = _hasEnteredDeteccion;
        if (shouldChase && _useRaycastDetection)
        {
            shouldChase = LineOfSight(tilemap, jugador);
        }

        float ySuelo = ObtenerYSueloDebajo(tilemap);
        if (ySuelo >= 2000f) ySuelo = 600f;

        if (shouldChase)
        {
            float alturaObjetivo = jugador.position.Y;
            float limiteVueloBajo = ySuelo - (Collider.Height / 2f) - 5f;
            if (alturaObjetivo > limiteVueloBajo)
            {
                alturaObjetivo = limiteVueloBajo;
            }

            float dirX = (jugador.position.X < this.position.X) ? -1 : 1;
            _velocity.X = dirX * _speed;
            efecto = (dirX == -1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float diferenciaY = alturaObjetivo - this.position.Y;
            _velocity.Y = MathHelper.Clamp(diferenciaY * 0.08f, -3.2f, 3.2f);
            MoverConFisicas(tilemap);
        }
        else
        {
            _velocity = Vector2.Zero;
        }

        _timerAnimacion += dt;
        if (_timerAnimacion >= _tiempoPorFrame)
        {
            _frameActual = (_frameActual + 1) % _totalFrames;
            _timerAnimacion = 0;
        }

        DetectarDano(jugador);
    }

    /// <summary>
    /// Aplica el vector de velocidad actual a la posición del enemigo con resolución
    /// de colisiones contra el tilemap en los ejes X e Y de forma independiente.
    /// Si se detecta colisión en un eje, se revierte el movimiento en ese eje
    /// y se anula la velocidad correspondiente para evitar que el enemigo se quede atascado.
    /// </summary>
    /// <param name="tilemap">Mapa de tiles de la escena actual.</param>
    private void MoverConFisicas(Dictionary<Vector2, int> tilemap)
    {
        float oldX = position.X;
        position.X += _velocity.X;
        SincronizarCollider();
        if (TocandoBloque(tilemap)) { position.X = oldX; _velocity.X = 0; }

        float oldY = position.Y;
        position.Y += _velocity.Y;
        SincronizarCollider();
        if (TocandoBloque(tilemap)) { position.Y = oldY; _velocity.Y = 0; }
    }

    /// <summary>
    /// Comprueba si el enemigo está en contacto con el jugador y, en caso afirmativo,
    /// le resta una vida y restaura la posición del enemigo a su punto de spawn.
    /// No produce daño si el jugador tiene el escudo activo (<see cref="MovedSprite.EnEstadoS"/>).
    /// Utiliza hitboxes reducidas para mayor precisión en el contacto percibido.
    /// </summary>
    /// <param name="jugador">Referencia al sprite del jugador.</param>
    private void DetectarDano(MovedSprite jugador)
    {
        if (jugador.EnEstadoS) return;

        Rectangle hurtboxM = GetRectReducido(0.5f);

        Rectangle hurtboxJ = new Rectangle(
            (int)jugador.Collider.Position.X,
            (int)jugador.Collider.Position.Y,
            jugador.Collider.Width,
            jugador.Collider.Height
        );
        hurtboxJ.Inflate(-5, -5);

        if (hurtboxM.Intersects(hurtboxJ))
        {
            VidaManager.PerderVida();
            ResetearPosicion();
        }
    }

    /// <summary>
    /// Restaura al enemigo a su posición de spawn original y anula su velocidad.
    /// Se llama automáticamente tras infligir daño al jugador y desde el evento
    /// <c>Respawn</c> de la escena cuando el jugador pierde una vida.
    /// </summary>
    public void ResetearPosicion()
    {
        this.position = _posicionInicial;
        this._velocity = Vector2.Zero;
        SincronizarCollider();
    }

    /// <summary>
    /// Actualiza la posición del <see cref="Collider"/> para que coincida con
    /// el centro del sprite más el <see cref="_colliderOffset"/> configurado.
    /// Debe llamarse cada vez que cambie <see cref="ScaledSprite.position"/>.
    /// </summary>
    private void SincronizarCollider()
    {
        Collider.Position = new Vector2(
            position.X - Collider.Width / 2f + _colliderOffset.X,
            position.Y - Collider.Height / 2f + _colliderOffset.Y);
    }

    /// <summary>
    /// Establece un desplazamiento manual del colisionador respecto al centro
    /// del sprite y sincroniza su posición inmediatamente.
    /// Útil para ajustar la hitbox en tiempo de ejecución sin recompilar.
    /// </summary>
    /// <param name="offsetX">Desplazamiento horizontal en píxeles.</param>
    /// <param name="offsetY">Desplazamiento vertical en píxeles.</param>
    public void SetColliderOffset(float offsetX, float offsetY)
    {
        _colliderOffset = new Vector2(offsetX, offsetY);
        SincronizarCollider();
    }

    /// <summary>
    /// Desplaza el offset del colisionador de forma incremental respecto
    /// a su valor actual y sincroniza la posición inmediatamente.
    /// </summary>
    /// <param name="delta">Vector de desplazamiento incremental en píxeles.</param>
    public void MoveCollider(Vector2 delta)
    {
        _colliderOffset += delta;
        SincronizarCollider();
    }

    /// <summary>
    /// Devuelve un <see cref="Rectangle"/> reducido proporcionalmente al colisionador
    /// principal. Se usa como hurtbox para que el contacto con el jugador resulte
    /// más preciso visualmente.
    /// </summary>
    /// <param name="porcentaje">Factor de reducción (0.0–1.0) aplicado a ancho y alto.</param>
    /// <returns>Rectángulo reducido alineado con la esquina superior izquierda del colisionador.</returns>
    private Rectangle GetRectReducido(float porcentaje)
    {
        int w = (int)(Collider.Width * porcentaje);
        int h = (int)(Collider.Height * porcentaje);
        return new Rectangle((int)Collider.Position.X, (int)Collider.Position.Y, w, h);
    }

    /// <summary>
    /// Comprueba si el colisionador del enemigo se solapa con algún tile sólido
    /// del tilemap. Los tiles con valor 18 (decorativo) y 99 (meta) se excluyen
    /// de la comprobación por no ser físicamente sólidos.
    /// </summary>
    /// <param name="tilemap">Mapa de tiles de la escena actual.</param>
    /// <returns><c>true</c> si hay solapamiento con al menos un tile sólido.</returns>
    private bool TocandoBloque(Dictionary<Vector2, int> tilemap)
    {
        Rectangle rEnemigo = new Rectangle(
            (int)Collider.Position.X, (int)Collider.Position.Y,
            Collider.Width, Collider.Height);

        foreach (var tile in tilemap)
        {
            if (tile.Value != 18 && tile.Value != 99)
            {
                Rectangle rBloque = new Rectangle(
                    (int)tile.Key.X * _tileSize, (int)tile.Key.Y * _tileSize,
                    _tileSize, _tileSize);
                if (rEnemigo.Intersects(rBloque)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Lanza un rayo discreto desde la posición del enemigo hasta la del jugador
    /// y determina si existe línea de visión directa sin tiles sólidos intermedios.
    /// El rayo se subdivide en pasos proporcionales al tamaño del tile.
    /// </summary>
    /// <param name="tilemap">Mapa de tiles de la escena actual.</param>
    /// <param name="jugador">Referencia al sprite del jugador.</param>
    /// <returns>
    /// <c>true</c> si no hay ningún tile sólido entre el enemigo y el jugador;
    /// <c>false</c> si la visión está bloqueada.
    /// </returns>
    private bool LineOfSight(Dictionary<Vector2, int> tilemap, MovedSprite jugador)
    {
        Vector2 a = this.position;
        Vector2 b = jugador.position;
        Vector2 delta = b - a;
        float dist = delta.Length();
        int steps = Math.Max(1, (int)(dist / _tileSize));
        Vector2 step = delta / steps;
        Vector2 pos = a;

        for (int i = 0; i <= steps; i++)
        {
            int tx = (int)Math.Floor(pos.X / _tileSize);
            int ty = (int)Math.Floor(pos.Y / _tileSize);
            Vector2 key = new Vector2(tx, ty);
            if (tilemap.TryGetValue(key, out int val))
            {
                if (val != 18 && val != 99)
                    return false;
            }
            pos += step;
        }
        return true;
    }

    /// <summary>
    /// Recalcula las dimensiones del <see cref="Collider"/> aplicando <paramref name="factor"/>
    /// sobre el tamaño del frame escalado al mundo. Garantiza un mínimo de 4 píxeles
    /// por lado para evitar colisionadores degenerados.
    /// </summary>
    /// <param name="factor">Factor de escala del colisionador respecto al frame (0.0–1.0).</param>
    private void UpdateColliderSize(float factor)
    {
        int anchoFrameWorld = (int)(_anchoFrame * scale);
        int altoFrameWorld = (int)(_altoFrame * scale);
        int side = (int)(Math.Min(anchoFrameWorld, altoFrameWorld) * factor);
        if (side < 4) side = 4;
        Collider.Width = side;
        Collider.Height = side;
        SincronizarCollider();
    }

    /// <summary>
    /// Busca el tile sólido más cercano por debajo del enemigo en su misma columna
    /// del tilemap. El resultado se usa para calcular la altura de vuelo objetivo,
    /// de forma que el enemigo siempre flote a una distancia constante del suelo.
    /// </summary>
    /// <param name="tilemap">Mapa de tiles de la escena actual.</param>
    /// <returns>
    /// Coordenada Y en píxeles del tile sólido más cercano por debajo.
    /// Devuelve 3000 si no se encuentra ninguno (valor de seguridad).
    /// </returns>
    private float ObtenerYSueloDebajo(Dictionary<Vector2, int> tilemap)
    {
        int colX = (int)Math.Floor(this.position.X / _tileSize);
        float yEncontrada = 3000f;

        foreach (var tile in tilemap)
        {
            if ((int)tile.Key.X == colX && tile.Value != 18 && tile.Value != 99)
            {
                float yPixel = tile.Key.Y * _tileSize;
                if (yPixel > this.position.Y - 20 && yPixel < yEncontrada)
                {
                    yEncontrada = yPixel;
                }
            }
        }
        return yEncontrada;
    }

    /// <summary>
    /// Dibuja el frame actual de la animación del enemigo centrado en su posición.
    /// Si <see cref="_debugMode"/> está activo, emite información de estado
    /// por la consola de depuración.
    /// </summary>
    /// <param name="spriteBatch">El <see cref="SpriteBatch"/> activo en el que se dibuja.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
        Vector2 origen = new Vector2(_anchoFrame / 2f, _altoFrame / 2f);
        spriteBatch.Draw(texture, position, fuente, Color.White, 0f, origen, scale, efecto, 0f);

        if (_debugMode)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[Enemigo Debug] pos={position}, coll={Collider.Width}x{Collider.Height}, " +
                $"distDet={_detectedRadius}, hasEntered={_hasEnteredDeteccion}");
        }
    }

    /// <summary>Activa o desactiva el modo de detección por línea de visión (raycast).</summary>
    /// <param name="value"><c>true</c> para requerir visión directa antes de perseguir.</param>
    public void SetRaycastDetection(bool value) { _useRaycastDetection = value; }

    /// <summary>Activa o desactiva la salida de información de depuración.</summary>
    /// <param name="value"><c>true</c> para habilitar el modo debug.</param>
    public void SetDebugMode(bool value) { _debugMode = value; }

    /// <summary>Factor de tamaño actual del colisionador respecto al frame escalado.</summary>
    public float ColliderSizeFactor => _colliderSizeFactor;

    /// <summary>
    /// Establece un nuevo factor de tamaño para el colisionador y lo recalcula inmediatamente.
    /// </summary>
    /// <param name="value">Nuevo factor (0.0–1.0).</param>
    public void SetColliderSizeFactor(float value) { _colliderSizeFactor = value; UpdateColliderSize(_colliderSizeFactor); }

    /// <summary>Velocidad de desplazamiento horizontal del enemigo en píxeles por frame.</summary>
    public float Speed { get => _speed; set => _speed = value; }

    /// <summary>Radio de detección del jugador en píxeles.</summary>
    public float DetectedRadius { get => _detectedRadius; set => _detectedRadius = value; }

    /// <summary>Indica si la detección por raycast está habilitada.</summary>
    public bool UseRaycastDetectionPublic { get => _useRaycastDetection; set => _useRaycastDetection = value; }

    /// <summary>Indica si el modo debug está habilitado (acceso de escritura alternativo).</summary>
    public bool DebugModePublic { get => _debugMode; set => _debugMode = value; }

    /// <summary>Indica si el modo debug está habilitado (acceso de solo lectura).</summary>
    public bool DebugMode => _debugMode;
    /// <summary>Resetea el estado de detección para que el enemigo vuelva a idle tras un respawn.</summary>
    public void ResetearDeteccion()
    {
        _hasEnteredDeteccion = false;
    }
}