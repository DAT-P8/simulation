using Godot;
using GWSimulation;

namespace Simulation.GridEnvironment;

public class GWDrone(StaticBody3D staticBody3D, long id)
{
    private readonly StaticBody3D _staticBody3D = staticBody3D;
    private readonly long _id = id;

    private int x = 0;
    private int y = 0;
    private int z = 0;

    public StaticBody3D StaticBody3D => _staticBody3D;
    public long Id => _id;
    public bool Destroyed { get; set; }

    public int X
    {
        get
        {
            return x;
        }
        set
        {
            x = value;
            _staticBody3D.Position = new Vector3(x, y, z);
        }
    }
    public int Y
    {
        get
        {
            return y;
        }
        set
        {
            y = value;
            _staticBody3D.Position = new Vector3(x, y, z);
        }
    }
    public int Z
    {
        get
        {
            return z;
        }
        set
        {
            z = value;
            _staticBody3D.Position = new Vector3(x, y, z);
        }
    }

    public GWDroneState GetState()
    {
        return new GWDroneState {
            Id = _id,
            X = x,
            Y = y,
            Destroyed = Destroyed
        };
    }
}
