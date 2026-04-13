using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace capybara
{
    /// <summary>
    /// Sprite del jugador con movimiento, físicas, salto doble y la mecánica
    /// de escudo de burbuja. Hereda de <see cref="ScaledSprite"/> y añade
    /// un <see cref="BoxCollider"/> propio, gestión de velocidad, animaciones
    /// de entrada y salida del escudo, y resolución de colisiones contra el
    /// <see cref="CollisionManager"/>.
    /// </summary>
    public class MovedSprite : ScaledSprite
    {
        /// <summary>Velocidad de desplazamiento horizontal en píxeles por frame.</summary>
        private float speed;

        /// <summary>
        /// Vector de velocidad actual del jugador.
        /// La componente X se asigna directamente desde la entrada del teclado;
        /// la componente Y acumula gravedad y es modificada por el salto.
        /// </summary>
        public new Vector2 velocity;

        /// <summary>Estado del teclado en el frame anterior (uso interno del método Update).</summary>
        private KeyboardState prevKeyboard;

        /// <summary>
        /// Número de saltos realizados desde la última vez que el jugador tocó el suelo.
        /// Se permite un máximo de 2 (salto doble). Se resetea a 0 al aterrizar.
        /// </summary>
        public int saltos = 0;

        /// <summary>Escala horizontal actual del sprite, interpolada suavemente hacia <see cref="escalaObjetivo"/>.</summary>
        private float escalaX = 1f;

        /// <summary>Escala horizontal objetivo (1 mirando a la derecha, -1 a la izquierda).</summary>
        private float escalaObjetivo = 1f;

        /// <summary>Velocidad de interpolación del giro horizontal del sprite.</summary>
        private float velocidadFlip = 8f;

        /// <summary>
        /// Sprite sheet de la animación de entrada al escudo de burbuja.
        /// Se dibuja en lugar del sprite base mientras <see cref="EnEstadoS"/> es <c>true</c>.
        /// </summary>
        public Texture2D TexEspecial { get; private set; }

        /// <summary>
        /// Sprite sheet de la animación de salida del escudo de burbuja.
        /// Se dibuja mientras <see cref="SaliendoDeS"/> es <c>true</c>.
        /// </summary>
        public Texture2D TexSalida { get; private set; }

        /// <summary>
        /// Indica si el escudo de burbuja está activo. Mientras lo está, el jugador
        /// no puede moverse horizontalmente, no recibe daño del enemigo y se reproduce
        /// la animación de <see cref="TexEspecial"/>. Se desactiva automáticamente
        /// tras 3 segundos.
        /// </summary>
        public bool EnEstadoS { get; private set; } = false;

        /// <summary>
        /// Indica si el jugador está en la fase de salida del escudo.
        /// Durante esta fase se reproduce la animación de <see cref="TexSalida"/>
        /// y el movimiento horizontal permanece bloqueado hasta que concluye.
        /// </summary>
        public bool SaliendoDeS { get; private set; } = false;

        /// <summary>Índice del frame actualmente visible en la animación del escudo (entrada o salida).</summary>
        public int FrameActualS { get; private set; } = 0;

        /// <summary>Número total de frames de la animación de entrada al escudo.</summary>
        public int TotalFramesS { get; private set; } = 7;

        /// <summary>Número total de frames de la animación de salida del escudo.</summary>
        public int TotalFramesSalida { get; private set; } = 7;

        /// <summary>Acumulador de tiempo para la duración total del escudo (máximo 3 segundos).</summary>
        private float _timerS = 0f;

        /// <summary>Acumulador de tiempo para el avance de frames de la animación del escudo.</summary>
        private float _timerAnimacion = 0f;

        /// <summary>
        /// Colisionador AABB del jugador. Se sincroniza cada frame con el rectángulo
        /// visual <see cref="ScaledSprite.Rect"/> a través de <see cref="ActualizarCollider"/>.
        /// </summary>
        public BoxCollider Collider { get; set; }

        /// <summary>
        /// Corrección de posición en X aplicada al girar el sprite para compensar
        /// el desplazamiento visual producido por el cambio de <see cref="SpriteEffects"/>.
        /// </summary>
        private float offsetCompensacionGiro = 20f;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MovedSprite"/>.
        /// Crea el <see cref="BoxCollider"/> inicial de 70×70 px y lo etiqueta
        /// como "player" para el sistema de filtrado de colisiones.
        /// </summary>
        /// <param name="texture">Sprite base del jugador (estado idle/walk).</param>
        /// <param name="position">Posición inicial en coordenadas de mundo.</param>
        /// <param name="scale">Factor de escala visual aplicado al sprite.</param>
        /// <param name="speed">Velocidad de desplazamiento horizontal en píxeles por frame.</param>
        /// <param name="texEspecial">Sprite sheet de la animación de entrada al escudo.</param>
        /// <param name="texSalida">Sprite sheet de la animación de salida del escudo.</param>
        public MovedSprite(Texture2D texture, Vector2 position, float scale, float speed,
                           Texture2D texEspecial, Texture2D texSalida)
            : base(texture, position, scale)
        {
            this.speed = speed;
            this.TexEspecial = texEspecial;
            this.TexSalida = texSalida;
            this.velocity = Vector2.Zero;
            if (Collider == null)
            {
                Collider = new BoxCollider(position, 70, 70, false);
            }
            Collider.Owner = "player";
        }

        /// <summary>
        /// Actualiza la lógica del jugador cada frame: gestiona la activación y
        /// duración del escudo de burbuja, procesa la entrada de movimiento y salto,
        /// aplica la compensación de giro y delega la física y resolución de
        /// colisiones en <see cref="AplicarFisicasYColisiones"/>.
        /// <para>
        /// Controles:
        /// <list type="bullet">
        ///   <item><description>A / D — movimiento horizontal izquierda / derecha.</description></item>
        ///   <item><description>W — salto (máximo 2 saltos consecutivos).</description></item>
        ///   <item><description>S — activa el escudo de burbuja (una vez por activación, duración 3 s).</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="keyboard">Estado del teclado en el frame actual.</param>
        /// <param name="teclaanterior">Estado del teclado en el frame anterior.</param>
        /// <param name="gravedad">Aceleración gravitacional en píxeles por frame².</param>
        /// <param name="fuerza">Velocidad Y inicial del salto (valor negativo).</param>
        /// <param name="gameTime">Información de tiempo del frame actual proporcionada por MonoGame.</param>
        public void Update(KeyboardState keyboard, KeyboardState teclaanterior, float gravedad, float fuerza, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Activación del escudo: solo si no está ya activo ni en fase de salida
            if (keyboard.IsKeyDown(Keys.S) && !EnEstadoS && !SaliendoDeS && teclaanterior.IsKeyUp(Keys.S))
            {
                EnEstadoS = true;
                _timerS = 0f;
                FrameActualS = 0;
                CollisionManager.IgnorePlayerEnemyCollisions = true;
            }

            // Lógica durante el escudo activo
            if (EnEstadoS)
            {
                _timerS += dt;
                velocity.X = 0;

                _timerAnimacion += dt;
                if (_timerAnimacion >= 0.1f)
                {
                    if (FrameActualS < TotalFramesS - 1) FrameActualS++;
                    _timerAnimacion = 0f;
                }

                // El escudo expira tras 3 segundos y pasa a la fase de salida
                if (_timerS >= 3f)
                {
                    EnEstadoS = false;
                    SaliendoDeS = true;
                    FrameActualS = 0;
                    CollisionManager.IgnorePlayerEnemyCollisions = false;
                }

                AplicarFisicasYColisiones(gravedad);
                prevKeyboard = keyboard;
            }

            // Lógica durante la animación de salida del escudo
            if (SaliendoDeS)
            {
                velocity.X = 0;
                _timerAnimacion += dt;
                if (_timerAnimacion >= 0.088f)
                {
                    FrameActualS++;
                    _timerAnimacion = 0f;
                }

                if (FrameActualS >= TotalFramesSalida)
                {
                    SaliendoDeS = false;
                }

                AplicarFisicasYColisiones(gravedad);
                prevKeyboard = keyboard;
            }

            SpriteEffects efectoAnterior = efecto;

            if (keyboard.IsKeyDown(Keys.D))
            {
                velocity.X = speed;
                escalaObjetivo = 1f;
                efecto = SpriteEffects.None;
            }
            else if (keyboard.IsKeyDown(Keys.A))
            {
                velocity.X = -speed;
                escalaObjetivo = -1f;
                efecto = SpriteEffects.FlipHorizontally;
            }
            else
            {
                velocity.X = 0;
            }

            // Compensación de posición al girar para evitar desplazamiento visual brusco
            if (efectoAnterior == SpriteEffects.None && efecto == SpriteEffects.FlipHorizontally)
                position.X -= offsetCompensacionGiro;
            else if (efectoAnterior == SpriteEffects.FlipHorizontally && efecto == SpriteEffects.None)
                position.X += offsetCompensacionGiro;

            escalaX += (escalaObjetivo - escalaX) * velocidadFlip * 0.016f;

            if (keyboard.IsKeyDown(Keys.W) && saltos < 2 && teclaanterior.IsKeyUp(Keys.W))
            {
                saltos++;
                velocity.Y = fuerza;
            }

            AplicarFisicasYColisiones(gravedad);

            prevKeyboard = keyboard;
        }

        /// <summary>
        /// Aplica gravedad, mueve al jugador y resuelve colisiones contra el tilemap
        /// de forma separada en los ejes Y y X para evitar falsos positivos en esquinas.
        /// <para>
        /// Orden de resolución:
        /// <list type="number">
        ///   <item><description>Se acumula gravedad en <c>velocity.Y</c>.</description></item>
        ///   <item><description>Se intenta el movimiento vertical; si hay colisión se revierte y se resetea <see cref="saltos"/>.</description></item>
        ///   <item><description>Se intenta el movimiento horizontal; si hay colisión se revierte.</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gravedad">Aceleración gravitacional en píxeles por frame² que se suma a <c>velocity.Y</c>.</param>
        private void AplicarFisicasYColisiones(float gravedad)
        {
            velocity.Y += gravedad;

            float oldPosY = position.Y;
            position.Y += velocity.Y;
            ActualizarCollider();

            if (HayColisionY())
            {
                position.Y = oldPosY;
                ActualizarCollider();
                if (velocity.Y > 0) saltos = 0;
                velocity.Y = 0;
            }

            float oldPosX = position.X;
            position.X += velocity.X;
            ActualizarCollider();

            if (HayColisionY())
            {
                position.X = oldPosX;
                ActualizarCollider();
            }
        }

        /// <summary>
        /// Sincroniza la posición y dimensiones del <see cref="Collider"/> con el
        /// rectángulo visual actual del sprite (<see cref="ScaledSprite.Rect"/>).
        /// Debe llamarse después de cualquier cambio en <see cref="ScaledSprite.position"/>.
        /// </summary>
        private void ActualizarCollider()
        {
            if (Collider != null)
            {
                Rectangle r = this.Rect;
                Collider.Position = new Vector2(r.X, r.Y);
                Collider.Width = r.Width;
                Collider.Height = r.Height;
            }
        }

        /// <summary>
        /// Comprueba si el <see cref="Collider"/> del jugador se solapa con algún
        /// colisionador registrado en el <see cref="CollisionManager"/>, excluyendo
        /// el propio collider del jugador y, cuando el escudo está activo, los
        /// colisionadores etiquetados como "enemy".
        /// </summary>
        /// <returns>
        /// <c>true</c> si existe al menos una intersección con un colisionador ajeno;
        /// <c>false</c> en caso contrario.
        /// </returns>
        private bool HayColisionY()
        {
            if (Collider == null) return false;
            foreach (var col in CollisionManager.GetColliders())
            {
                if (col == Collider) continue;

                if (CollisionManager.IgnorePlayerEnemyCollisions && col.Owner == "enemy")
                    continue;

                if (Collider.Intersects(col))
                    return true;
            }
            return false;
        }
    }
}