using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace capybara
{
    public class SettingsScene : IScene
    {
        private SceneManager _sceneManager;
        private ContentManager _content;
        private GraphicsDevice _graphicsDevice;
        private float _volumenActual;
        private Texture2D _fondoLiso;
        private Texture2D _texBotonSubir;
        private Texture2D _texBotonBajar;
        private Texture2D _texBotonMenu;
        private Rectangle _rectSubir;
        private Rectangle _rectBajar;
        private Rectangle _rectMenu;

        private Rectangle colSubir;
        private Rectangle colBajar;
        private Rectangle colMenu;
        private MouseState _mouseAnterior;

        private Texture2D _texvolumen;
        private Rectangle _rectVolumenDibujo;
        private SpriteFont _fuente;

       public SettingsScene(SceneManager sceneManager, ContentManager content, GraphicsDevice graphicsDevice)
{
    _sceneManager = sceneManager;
    _content = content;
    _graphicsDevice = graphicsDevice;
    _mouseAnterior = Mouse.GetState();

   
    int tamBoton = 120;     
    int anchoVol = 500;     
    int altoVol = 200;       
    int anchoSalir = 500;    
    int altoSalir = 200;     
    
    int centroX = 1280 / 2;
    int centroY = 720 / 2;

    
    _rectVolumenDibujo = new Rectangle(centroX - (anchoVol / 2), centroY - 120, anchoVol, altoVol);

    _rectBajar = new Rectangle(_rectVolumenDibujo.X - tamBoton - 30, _rectVolumenDibujo.Y + (altoVol/2 - tamBoton/2), tamBoton, tamBoton);

   
    _rectSubir = new Rectangle(_rectVolumenDibujo.Right + 30, _rectVolumenDibujo.Y + (altoVol/2 - tamBoton/2), tamBoton, tamBoton);

   
    _rectMenu = new Rectangle(centroX - (anchoSalir / 2), _rectVolumenDibujo.Bottom + 10, anchoSalir, altoSalir);

   
    colBajar = _rectBajar;
    colBajar.Inflate(-15, -15); 

    colSubir = _rectSubir;
    colSubir.Inflate(-15, -15);

    colMenu = _rectMenu;
    colMenu.Inflate(-20, -20);
}

        public void CambiarVolumen(float cantidad)
        {
            _volumenActual = MathHelper.Clamp(_volumenActual + cantidad, 0.0f, 1.0f);
            MediaPlayer.Volume = _volumenActual;
            SoundEffect.MasterVolume = _volumenActual;
            System.Diagnostics.Debug.WriteLine($"Volumen: {_volumenActual}");
        }

        public void LoadContent()
        {
            _fondoLiso = new Texture2D(_graphicsDevice, 1, 1);
            _fondoLiso.SetData(new[] { new Color(10, 10, 10, 230) });

            _texBotonSubir = _content.Load<Texture2D>("subir_volumen");
            _texBotonBajar = _content.Load<Texture2D>("bajar_volumen");
            _texBotonMenu = _content.Load<Texture2D>("boton_menu");
            _texvolumen = _content.Load<Texture2D>("volumen");
            _fuente = _content.Load<SpriteFont>("Fuente");

            _volumenActual = MediaPlayer.Volume;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseActual = Mouse.GetState();
            Point mousePos = mouseActual.Position;

            bool clic = mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released;

            if (clic)
            {
                if (colSubir.Contains(mousePos))
                {
                    CambiarVolumen(0.1f);
                }
                else if (colBajar.Contains(mousePos))
                {
                    CambiarVolumen(-0.1f);
                }
                else if (colMenu.Contains(mousePos))
                {
                    _sceneManager.RemoveScene();
                }
            }
            _mouseAnterior = mouseActual;
        }

     public void Draw(SpriteBatch spriteBatch)
{

    spriteBatch.Draw(_fondoLiso, new Rectangle(0, 0, 1280, 720), Color.White);

    spriteBatch.Draw(_texvolumen, _rectVolumenDibujo, Color.White);

   
    Point mPos = Mouse.GetState().Position;
    
    Color cBajar = colBajar.Contains(mPos) ? Color.Gray : Color.White;
    Color cSubir = colSubir.Contains(mPos) ? Color.Gray : Color.White;
    Color cMenu = colMenu.Contains(mPos) ? Color.Gray : Color.White;

    spriteBatch.Draw(_texBotonBajar, _rectBajar, cBajar);
    spriteBatch.Draw(_texBotonSubir, _rectSubir, cSubir);
    spriteBatch.Draw(_texBotonMenu, _rectMenu, cMenu);

    string texto = $"{(int)(_volumenActual * 100)}%";
    Vector2 tamTexto = _fuente.MeasureString(texto);
    Vector2 posTexto = new Vector2(1280 / 2 - tamTexto.X / 2, _rectMenu.Bottom + 20);
    spriteBatch.DrawString(_fuente, texto, posTexto, Color.White);
}
    }
}