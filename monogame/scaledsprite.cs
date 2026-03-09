using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
 //clase para el escalado del sprite, y para creear el rectangulo para el tema de las colisiones, que se va a usar en el futuro 
namespace capybara
{
    internal class ScaledSprite : Sprite
{
    public float scale; // necesitamos guardar la escala

    public Rectangle Rect {
    get {
        float reduccion = 0.6f; // ← ajusta este valor (0.5 = mitad, 0.8 = casi completo)
        int ancho = (int)(texture.Width * scale * reduccion);
        int alto = (int)(texture.Height * scale * reduccion);
        return new Rectangle(
            (int)Math.Round(position.X - (ancho*1.3) / 2f),
            (int)Math.Round(position.Y - alto / 2f),
            ancho,
            alto
        );
    }
}

    public ScaledSprite(Texture2D texture, Vector2 position, float scale) 
        : base(texture, position)
    {
        this.scale = scale; // ← guardar la escala
    }
}
}