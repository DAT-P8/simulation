using System;

namespace Simulation.GridEnvironment;

public class GWEnvData(int mapSize)
{
    private readonly bool oddMapSize = (mapSize & 1) == 1;
    private float offset => oddMapSize ? 0.0f : 0.5f;
    private readonly int halfSize = (int)Math.Floor(mapSize / 2.0);
    private readonly int mapSize = mapSize;
    private (float, float) targetArea => (-offset, -offset);

    public Func<float,float> ApplyOffset => pos => pos < 0 ? pos + offset: pos - offset;
    public Func<(float,float), bool> IsInTarget => pos => pos == targetArea;

    public bool IsOdd => oddMapSize;
    public float GetOffset => offset;
    public int GetHalfSize => halfSize;
    public int GetMapSize => mapSize;
    public (float, float) GetTargetPosition => targetArea;
}
