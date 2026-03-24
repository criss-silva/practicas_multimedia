using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace capybara;

public class HUD
{
    private Texture2D _vidas3;
    private Texture2D _vidas2;
    private Texture2D _vidas1; 
    private Rectangle _rectVidas;
    public HUD()
    {
        // esquina superior derecha
        _rectVidas = new Rectangle(1280 - 160, -200, 160, 60);
    }

    public void LoadContent(ContentManager content)
    {
        _vidas3 = content.Load<Texture2D>("vidas");
         _vidas2 = content.Load<Texture2D>("2vidas");
         _vidas1 = content.Load<Texture2D>("1vida");
        float escala = 3.0f; //como la imagen es muy pequeña debemos crear un factor de escala
        int ancho = (int)(_vidas3.Width * escala);
        int alto = (int)(_vidas3.Height * escala);
        _rectVidas = new Rectangle(1280 - ancho - 10, -50, ancho, alto);
    }

    public void Draw(SpriteBatch spriteBatch)
    {

        if(VidaManager.Vidas==0)return;

        Texture2D spriteVidas = VidaManager.Vidas switch
        {
            3 => _vidas3,
            2 => _vidas1,
            1 => _vidas2,
            _ => _vidas2 // fallback mientras no tengas el sprite de 1 vida
        };
        spriteBatch.Draw(spriteVidas, _rectVidas, Color.White);
    }
}