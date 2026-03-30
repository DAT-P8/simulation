using Godot;

namespace Simulation.GridEnvironment;

// Y should probably just be removed since it's always 0

/// <summary>
/// A record representing a drones position on the map grid
/// </summary>
public record GWPosition(int X, int Y, int Z, GWEnvData EnvData){
    /// <summary>
    /// Gives the grid position as a 3D-Vector representing the actual position in Godot
    /// </summary>
    public Vector3 ToVector() => new(EnvData.ApplyOffset(X), 1, EnvData.ApplyOffset(Z));
};
