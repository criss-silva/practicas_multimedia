using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace capybara
{
    public class AnimacionFondo 
    {
        private Texture2D _spriteSheet;
        private int _totalFrames;
        private int _frameActual = 0;
        private float _tiempoPorFrame;
        private float _cronometro = 0f;
        private int _anchoFrame;
        private int _altoFrame;

        public AnimacionFondo(Texture2D spriteSheet, int totalFrames, float fps = 8f)
        {
            _spriteSheet = spriteSheet;
            _totalFrames = totalFrames;
            _tiempoPorFrame = 1f / fps;
            _anchoFrame = spriteSheet.Width / totalFrames;
            _altoFrame = spriteSheet.Height;
        }

        public void Update(GameTime gameTime)
        {
            _cronometro += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cronometro >= _tiempoPorFrame)
            {
                _frameActual = (_frameActual + 1) % _totalFrames;
                _cronometro = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle destino)
        {
            Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
            spriteBatch.Draw(_spriteSheet, destino, fuente, Color.White);
        }
    }
}