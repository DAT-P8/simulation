using Godot;
using GW2D.V1;

public static class DtoMapper
{
    public static Vector3I ToVector(Action action)
    {
        return action switch
        {
            Action.Down => new(0, -1, 0),
            Action.LeftDown => new(-1, -1, 0),
            Action.Left => new(-1, 0, 0),
            Action.LeftUp => new(-1, 1, 0),
            Action.Up => new(0, 1, 0),
            Action.RightUp => new(1, 1, 0),
            Action.Right => new(1, 0, 0),
            Action.RightDown => new(1, -1, 0),
            Action.Nothing => new(0, 0, 0),
            Action.UnknownUnspecified => throw new System.Exception("Received unspecified action"),
            _ => throw new System.Exception($"Did not recognize action: {action}!"),
        };
    }
}
