using Godot;

namespace Simulation.Utils;

public class SceneObjectGetter
{
    public static T GetObject<T>()
    {
        var children = Main.MainScene.GetChildren();
        foreach (var c in children)
        {
            if (c is T tChild)
                return tChild;
        }
        throw new System.Exception($"Did not find any children of type {typeof(T)}");
    }
}
