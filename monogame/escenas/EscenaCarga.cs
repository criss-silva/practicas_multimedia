using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    /// <summary>
    /// Pantalla de carga entre el Mundo 1 y el Mundo 2 en el modo secuencial.
    /// Muestra un fondo de color plano aleatorio, el texto "Cargando..." en la
    /// parte inferior y la animación <c>animacion_romper_burbuja</c> centrada en
    /// pantalla. Tras <see cref="Duracion"/> segundos apila automáticamente
    /// <see cref="GameScene_M2"/> y se retira de la pila.
    /// </summary>
    public class EscenaCarga : IScene
    {
        // ── Dependencias ────────────────────────────────────────────────────
        private readonly SceneManager    _sceneManager;
        private readonly ContentManager  _content;
        private readonly GraphicsDevice  _graphicsDevice;

        /// <summary>Segundos que dura la pantalla de carga antes de avanzar.</summary>
        private const float Duracion = 2f;

        /// <summary>Número de frames horizontales del sprite sheet de la animación.</summary>
        private const int TotalFrames = 7;

        /// <summary>Segundos por frame de la animación (bucle continuo).</summary>
        private const float TiempoPorFrame = 0.1f;

        // ── Assets ───────────────────────────────────────────────────────────
        private Texture2D _pixel;
        private Texture2D _texAnimacion;
        private SpriteFont _fuente;

        // ── Estado ───────────────────────────────────────────────────────────
        private Color  _colorFondo;
        private float  _timerTotal;
        private float  _timerFrame;
        private int    _frameActual;
        private bool   _mundoCargado;

        // ── Geometría ────────────────────────────────────────────────────────
        private Rectangle _rectPantalla;

        public EscenaCarga(SceneManager sceneManager, ContentManager content, GraphicsDevice graphicsDevice)
        {
            _sceneManager   = sceneManager;
            _content        = content;
            _graphicsDevice = graphicsDevice;

            // Color plano aleatorio: tonos medios para que el texto blanco sea legible
            var rng = new Random();
            _colorFondo = new Color(
                rng.Next(60, 180),
                rng.Next(60, 180),
                rng.Next(60, 180));

            _rectPantalla = new Rectangle(0, 0,
                graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height);
        }

        public void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _texAnimacion = _content.Load<Texture2D>("animacion_romper_burbuja");

            // Intentamos cargar una fuente; si no existe usamos dibujo manual
            try { _fuente = _content.Load<SpriteFont>("fuente"); }
            catch { _fuente = null; }
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ── Avance de animación en bucle ─────────────────────────────────
            _timerFrame += dt;
            if (_timerFrame >= TiempoPorFrame)
            {
                _frameActual = (_frameActual + 1) % TotalFrames;
                _timerFrame  = 0f;
            }

            // ── Temporizador de duración ─────────────────────────────────────
            _timerTotal += dt;
            if (_timerTotal >= Duracion && !_mundoCargado)
            {
                _mundoCargado = true;

                // Cargamos el mundo 2 y lo apilamos; esta escena queda enterrada
                // y el SceneManager la dejará de actualizar/dibujar automáticamente.
                var mundo2 = new GameScene_M2(_sceneManager, _content, _graphicsDevice);
                mundo2.LoadContent();

                // Primero nos retiramos nosotros de la pila, luego apilamos el mundo.
                _sceneManager.RemoveScene();
                _sceneManager.AddScene(mundo2);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int w = _graphicsDevice.Viewport.Width;
            int h = _graphicsDevice.Viewport.Height;

            // ── Fondo plano ──────────────────────────────────────────────────
            spriteBatch.Draw(_pixel, _rectPantalla, _colorFondo);

            // ── Animación centrada ───────────────────────────────────────────
            int anchoFrame = _texAnimacion.Width  / TotalFrames;
            int altoFrame  = _texAnimacion.Height;

            // Escala 3× para que sea visible
            float escala    = 0.1f;
            int anchoDestino = (int)(anchoFrame * escala);
            int altoDestino  = (int)(altoFrame  * escala);

            Rectangle fuente = new Rectangle(_frameActual * anchoFrame, 0, anchoFrame, altoFrame);
            Rectangle destino = new Rectangle(
                (w - anchoDestino) / 2,
                (h - altoDestino)  / 2,
                anchoDestino,
                altoDestino);

            spriteBatch.Draw(_texAnimacion, destino, fuente, Color.White);

            // ── Texto "Cargando..." ──────────────────────────────────────────
            DibujarTexto(spriteBatch, "Cargando...", w, h);
        }

        // ────────────────────────────────────────────────────────────────────
        // Dibuja el texto "Cargando..." con la fuente si está disponible,
        // o con un rectángulo de píxeles simple como fallback visual.
        // ────────────────────────────────────────────────────────────────────
        private void DibujarTexto(SpriteBatch sb, string texto, int w, int h)
        {
            if (_fuente != null)
            {
                Vector2 tamTexto = _fuente.MeasureString(texto);
                Vector2 posTexto = new Vector2((w - tamTexto.X) / 2f, h - tamTexto.Y - 30f);
                // Sombra
                sb.DrawString(_fuente, texto, posTexto + new Vector2(2, 2), Color.Black * 0.6f);
                sb.DrawString(_fuente, texto, posTexto, Color.White);
            }
            else
            {
                // Fallback: barra de progreso simple bajo la animación
                int barW = 300;
                int barH = 12;
                int barX = (w - barW) / 2;
                int barY = h - 60;

                // Fondo de la barra
                sb.Draw(_pixel, new Rectangle(barX, barY, barW, barH), Color.Black * 0.5f);
                // Progreso
                float progreso = Math.Min(_timerTotal / Duracion, 1f);
                sb.Draw(_pixel, new Rectangle(barX, barY, (int)(barW * progreso), barH), Color.White);
            }
        }
    }
}