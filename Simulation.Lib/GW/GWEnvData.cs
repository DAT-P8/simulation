using System;
using Simulation.Lib.GW;

namespace Simulation.GridEnvironment;

public class GWEnvData(int mapSize, int targetX, int targetY)
{
    // Apply offset so drones positions match with the map in Godot
    private readonly bool oddMapSize = (mapSize & 1) == 1;
    private float offset => oddMapSize ? 0.0f : 0.5f;
    public Func<float,float> ApplyOffset => pos => pos < 0 ? pos + offset: pos - offset;

    // Specifications of the map dimensions
    private readonly int mapSize = mapSize;
    private readonly int halfSize = (int)Math.Floor(mapSize / 2.0);

    // Position of target related to the grid position
    private readonly int targetX = targetX;
    private readonly int targetY = targetY;

    // USed to check if a drone has entered the target area
    public Func<int, int, bool> IsInTarget => (x, y) => x == targetX && y == targetY;

    // Getters
    public int GetHalfSize => halfSize;
    public int GetMapSize => mapSize;
    public (int targetX, int targetY) GetTargetPosition => (targetX, targetY);
}

//public bool IsOdd => oddMapSize;
//public float GetOffset => offset;

//private (float, float) targetArea => (-offset, -offset);
//public Func<(float,float), bool> IsInTarget => pos => pos == targetArea;
