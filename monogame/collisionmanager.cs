using System.Collections.Generic;

namespace capybara
{
//¿que hacemos con las colisiones?
    public static class CollisionManager // no genera instancias por eso es estatica, solo es un gestor
    {
        private static List<BoxCollider> colliders = new List<BoxCollider>();
        private static List<BoxCollider> toRemove = new List<BoxCollider>();
        //tenemos dos listas porq daba error al borrar mientras trabajo con la lista, entonces los emtemos en toremove y luego de ahi borramos a la principal

        public static void AddCollider(BoxCollider collider)
        {
            colliders.Add(collider);
        }

        public static void RemoveCollider(BoxCollider collider)
        {
            toRemove.Add(collider);
        }

        public static void Clear() //imagina que cambiamos de escena, hay que borrar todo
        {
            toRemove.AddRange(colliders);
        }

        public static void Update()
        {
            CheckCollisions();//verifica colisiones- abajo

            foreach (var collider in toRemove) //pos a borrar 
            {
                colliders.Remove(collider);
                collider.ClearCollisions();
            }
            toRemove.Clear();
        }

        private static void CheckCollisions() //esto esta poco optimizado con dos bucles pero me habia cansado de pensar
        {
            foreach (var colliderA in colliders)
            {
                foreach (var colliderB in colliders)
                {
                    if (colliderA != colliderB && colliderA.IsActive && colliderB.IsActive)
                    {
                        colliderA.CheckCollision(colliderB); //recurisividad de la chula
                    }
                }
            }
        }
        public static List<BoxCollider> GetColliders()
            {
                return colliders;
            }

    }
    
}
