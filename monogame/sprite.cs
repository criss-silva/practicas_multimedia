using System.Text;
using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;


namespace capybara
{
    public class Sprite
    {
        //atributos esenciales para el personaje
        public Texture2D texture;
        public Vector2 position;//vamos a ir guardando su posición
        public Vector2 velocity;


        public Sprite(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
            this.velocity = Vector2.Zero;
        }

        public virtual void Update(){}   
        
    }
    
}