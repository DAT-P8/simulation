using System;
using System.Linq;
using Godot;

namespace Simulation.GridEnvironment.GridMaps;

public class GWBoxEnvData(int mapSize, int targetX, int targetY) : IGWEnvData
{
    // Offset so map is placed correctly if it has an odd size
    private readonly bool oddMapSize = (mapSize & 1) == 1;
    private float Offset => oddMapSize ? 0.0f : 0.5f;

    private Func<float, float> ApplyOffset => pos => pos < 0 ? pos + Offset : pos - Offset;
    private readonly int halfSize = (int)Math.Floor(mapSize / 2.0);

    public Vector3 GetMapPosition() => new(ApplyOffset(halfSize), 0, ApplyOffset(halfSize));

    public int GetMapSize => mapSize;

    public (int, int) GetTargetPosition()
    {
        return (targetX, targetY);
    }
    public bool IsInTarget(int x, int y)
    {
        return x >= 0 && x <= mapSize && y >= 0 && y <= mapSize;
    }
    public bool IsInBounds(int x, int y)
    {
        return x == targetX && y == targetY;
    }
    public GWPosition[] GetEvaderSpawns(int nEvaders)
    {
        Random random = new();

        GWPosition[] positions = new GWPosition[nEvaders];
        (bool, bool, int)[] usedPositions = [];
        for (int i = 0; i < nEvaders; i++)
        {
            bool lower, left;
            int rand;
            do
            {
                lower = (1 & random.Next()) == 1;
                left = (1 & random.Next()) == 1;
                rand = random.Next(0, mapSize);
            } while (!usedPositions.Contains((lower, left, rand)));

            GWPosition evader_position;
            if (lower)
                evader_position = new(!left ? rand : 0, 0, left ? rand : 0);
            else
                evader_position = new(!left ? rand : mapSize, 0, left ? rand : mapSize);

            positions[i] = evader_position;
        }

        return positions;
    }
    public GWPosition[] GetPursuerSpawns(int nPursuers)
    {
        Random random = new();

        GWPosition[] positions = new GWPosition[nPursuers];
        (int, int)[] usedPositions = [];
        for (int i = 0; i < nPursuers; i++)
        {
            int randX, randY;
            do
            {
                randX = random.Next(-1, 1);
                randY = random.Next(-1, 1);
                if (randX == 0 && randY == 0) // Drones cannot spawn on the target
                    continue;
            } while (!usedPositions.Contains((randX, randY)));

            GWPosition pusuer_position = new(targetX + randX, 0, targetY + randY);

            positions[i] = pusuer_position;
        }

        return positions;
    }
}
