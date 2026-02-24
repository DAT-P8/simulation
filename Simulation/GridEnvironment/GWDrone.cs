using Godot;
using GWSimulation;

namespace Simulation.GridEnvironment;

public class GWDrone(StaticBody3D staticBody3D, long id, bool isEvader)
{
    private readonly StaticBody3D _staticBody3D = staticBody3D;
    private readonly long _id = id;
    private readonly bool _isEvader = isEvader;

    public int X => _x;
    public int Y => _y;
    public int Z => _z;

    private int _x = 0;
    private int _y = 0;
    private int _z = 0;

    public StaticBody3D StaticBody3D => _staticBody3D;
    public bool IsEvader => _isEvader;
    public long Id => _id;
    public bool Destroyed { get; set; }

    public void SetPosition(GWPosition position)
    {
        _x = position.X;
        _y = position.Y;
        _z = position.Z;

        StaticBody3D.CallDeferred(Node3D.MethodName.SetPosition, new Vector3(position.X, position.Y, position.Z));
    }

    public GWPosition GetPosition() => new(_x, _y, _z);

    public GWDroneState GetState()
    {
        return new GWDroneState {
            Id = _id,
            X = _x,
            Y = _z,
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
