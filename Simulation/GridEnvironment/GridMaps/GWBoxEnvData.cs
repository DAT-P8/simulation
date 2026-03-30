using System;
using Godot;

namespace Simulation.GridEnvironment.GridMaps;

public class GWBoxEnvData(int mapSize, int targetX, int targetY) : IGWEnvData{
    // Offset so map is placed correctly if it has an odd size
    private readonly bool oddMapSize = (mapSize & 1) == 1;
    private float Offset => oddMapSize ? 0.0f : 0.5f;

    private Func<float,float> ApplyOffset => pos => pos < 0 ? pos + Offset: pos - Offset;
    private readonly int halfSize = (int)Math.Floor(mapSize / 2.0);

    public Vector3 GetMapPosition() => new(ApplyOffset(halfSize), 0, ApplyOffset(halfSize));

    public int GetMapSize => mapSize;

    public (int, int) GetTargetPosition(){
        return (targetX, targetY);
    }
    public bool IsInTarget(int x, int y){
        return x >= 0 && x <= mapSize && y >= 0 && y <= mapSize;
    }
    public bool IsInBounds(int x, int y){
        return x == targetX && y == targetY;
    }
}
