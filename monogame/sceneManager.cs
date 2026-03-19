using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace capybara{


public class SceneManager
{

    private  readonly Stack<IScene> stackescenas;
    public SceneManager()
    {
        stackescenas = new();
    }

    public void AddScene(IScene escena)
    {
        stackescenas.Push(escena);
    }

    public void RemoveScene()
    {
        if (stackescenas.Count > 0)
        {
            stackescenas.Pop();
        }
    }

    public IScene sceneaActual()
    {
        if (stackescenas.Count > 0)
        {
            return stackescenas.Peek();
        }
        return null;
    }
 
}
}