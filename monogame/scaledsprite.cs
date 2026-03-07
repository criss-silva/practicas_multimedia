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
        public Rectangle Rect  { //rectangulo para las colisiones
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, 100,200);
            }
          }
        public ScaledSprite(Texture2D texture, Vector2 position, float scale) : base(texture, position)
        {
           
        }
    }
}