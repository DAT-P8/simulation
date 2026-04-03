using Godot;

namespace Simulation.Instances;

public record GWPosition(int X, int Y, int Z)
{
    public Vector3 ToVector() => new(X, 0, Z);
};
