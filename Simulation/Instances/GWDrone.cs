using Godot;
using GW2D.V1;

namespace Simulation.Instances;

public class GWDrone(StaticBody3D staticBody3D, long id, bool isEvader)
{
    private readonly StaticBody3D _staticBody3D = staticBody3D;
    private readonly long _id = id;
    private readonly bool _isEvader = isEvader;

    public int X => _x;
    public int Y => _y;

    private int _x = 0;
    private int _y = 0;

    public StaticBody3D StaticBody3D => _staticBody3D;
    public bool IsEvader => _isEvader;
    public long Id => _id;
    public bool Destroyed { get; set; }

    public void SetPosition(Vector2I position)
    {
        _x = position.X;
        _y = position.Y;

        StaticBody3D.CallDeferred(Node3D.MethodName.SetPosition, new Vector3(_x, 0, _y));
    }

    public Vector2I GetPosition() => new(_x, _y);

    public DroneState GetState()
    {
        return new DroneState
        {
            Id = _id,
            X = _x,
            Y = _y,
            Destroyed = Destroyed,
            IsEvader = IsEvader
        };
    }

    public MeshInstance3D? GetMeshInstance()
    {
        foreach (var child in StaticBody3D.GetChildren())
        {
            if (child is MeshInstance3D meshInstance)
                return meshInstance;
        }

        return null;
    }
}
