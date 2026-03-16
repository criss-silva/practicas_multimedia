using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
//Clase para separar el collider y las colisiones 
namespace capybara
{
    public class BoxCollider
    {
        //esto es "delegar" es decir refiere a cualquier funcion que sea un void o reciba un BoxCollider como parametro
        public delegate void CollisionEvent(BoxCollider other);
        //esto está codigo de unity, lo que hacemos es dividr las colisiones por eventos
        public event CollisionEvent OnCollisionEnter; //esto es cuando dos collider se tocan por primera vez
        public event CollisionEvent OnCollisionStay; //esto es cuando ya se han tocado, cada frame mientras se tocan 
        public event CollisionEvent OnCollisionExit; //aqui dejan de tocarse
        //es como si hubiera un enum con tres estados, se tocan / tocandose / dejan de tocarse 
        //los nombramos como event porque así pueden coexistir y funcionar a la vez, se les puede añadir o quitar funciones con += o -=


        public bool IsTrigger; //esto será lo que detecte colisiones SIN HACER NADA, solo ¿se tocan?
        public bool IsActive = true; //activa o desactiva collider

        public Vector2 Position;
        public int Width;
        public int Height;

        private List<BoxCollider> currentCollisions = new List<BoxCollider>(); //todas las colisiones que haya

        public int Top { get { return (int)Position.Y; } }
        public int Bottom { get { return (int)Position.Y + Height; } }
        public int Left { get { return (int)Position.X; } }
        public int Right { get { return (int)Position.X + Width; } }

        public Vector2 Center //pos ea, el centro y lo de arriba las esquinas del collider
        {
            get { return Position + new Vector2(Width / 2, Height / 2); }
            set { Position = new Vector2(value.X - Width / 2, value.Y - Height / 2); }
        }
        //constructor parametrizado
        public BoxCollider(Vector2 position, int width, int height, bool isTrigger = false)
        {
            Position = position;
            Width = width;
            Height = height;
            IsTrigger = isTrigger;
        }
        //chocan o keloke
        public bool Intersects(BoxCollider other)
        {
            if (!IsActive || !other.IsActive)
                return false;

            return !(Right < other.Left || other.Right < Left ||
                     Bottom < other.Top || other.Bottom < Top);
        }
        //aqui van todas las funciones para las colisiones en las esquinas
        public bool CollidesWithTopOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy <= -hx && wy <= hx;
        }

        public bool CollidesWithBottomOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy > -hx && wy > hx;
        }

        public bool CollidesWithLeftOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy <= -hx && wy > hx;
        }

        public bool CollidesWithRightOf(BoxCollider other)
        {
            float wy = (Width + other.Width) * (Center.Y - other.Center.Y);
            float hx = (Height + other.Height) * (Center.X - other.Center.X);
            return wy > -hx && wy <= hx;
        }

        public void CheckCollision(BoxCollider other) //esto se llama frame a frame 
        {
            if (!Intersects(other)) //No se intersectan
            {
                if (currentCollisions.Contains(other))//¿se tocaban antes?
                {
                    if (OnCollisionExit != null) //si 
                        OnCollisionExit(other);
                    currentCollisions.Remove(other);
                }
                return;
            }
            //si intersectan, ahora verifica si se estaban tocando el segundo anteriore
            if (currentCollisions.Contains(other))
            {
                if (OnCollisionStay != null)
                    OnCollisionStay(other);
            }
            else //se tocan pero justo antes no, colision antes
            {
                currentCollisions.Add(other);
                if (OnCollisionEnter != null)
                    OnCollisionEnter(other);
            }
        } //bsicamente 1- se han dejado de tocar? 2.es una colision que venia de antes? 3. es colision nueva?

        public void ClearCollisions() //limpia colisiones
        {
            currentCollisions.Clear();
        }
    }
}
