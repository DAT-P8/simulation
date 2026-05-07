using Godot;
using GW2D.V1;

namespace Simulation.Instances;

public class GWDrone(StaticBody2D staticBody2D, long id, bool isEvader)
{
    private readonly Vector2 offsetVector = new(8, 8);
    private readonly StaticBody2D _staticBody2D = staticBody2D;
    private readonly long _id = id;
    private readonly bool _isEvader = isEvader;

    public int X => _x;
    public int Y => _y;

    private int _x = 0;
    private int _y = 0;

    public StaticBody2D StaticBody2D => _staticBody2D;
    public bool IsEvader => _isEvader;
    public long Id => _id;
    public bool Destroyed { get; set; }

    public void SetPosition(Vector2I position)
    {
        _x = position.X;
        _y = position.Y;

        // Offset position to fit visually
        Vector2 godotPosition = new Vector2(_x, _y) * 16;
        StaticBody2D.CallDeferred(Node2D.MethodName.SetPosition, godotPosition + offsetVector);
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
}
