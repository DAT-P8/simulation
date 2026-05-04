using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GW2D.V1;
using Serilog;
using Simulation.Utils;

namespace Simulation.Services;

public class PositionUtility(Random random, ILogger logger) : IPositionUtility
{
    private readonly Random _random = random;
    private readonly ILogger _logger = logger;

    public List<Vector3I> GetDefenderSpawn(MapSpec mapSpec, int count)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => GetDefenderSpawn(mapSpec.SquareMap, count),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot generate position(s)."),
            _ => throw new Exception($"Did not recognize map type: {mapSpec.MapOneofCase}"),
        };
    }

    public List<Vector3I> GetDefenderSpawn(SquareMap squareMap, int nDefenders)
    {
        // Make sure radius is big enough to spawn the amount of defenders
        int maxRadius = (int)Math.Ceiling(Math.Sqrt(nDefenders)) + 2;
        Vector3I targetPos = new((int)squareMap.TargetX, 0, (int)squareMap.TargetY);

        List<Vector3I> validSpawns = [];
        for (int i = -maxRadius; i <= maxRadius; i++)
        {
            for (int j = -maxRadius; j <= maxRadius; j++)
            {
                Vector3I pos = new Vector3I(i, 0, j) + targetPos;
                if (!ValidSpawn(squareMap, pos))
                    continue;
                validSpawns.Add(pos);
            }
        }
        validSpawns = Shuffler.Shuffle(_random, validSpawns);
        return [.. validSpawns.Take(nDefenders)];
    }

    public List<Vector3I> GetAttackerSpawn(MapSpec mapSpec, int count)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => GetAttackerSpawn(mapSpec.SquareMap, count),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot generate position(s)."),
            _ => throw new Exception($"Did not recognize map type: {mapSpec.MapOneofCase}"),
        };
    }

    public List<Vector3I> GetAttackerSpawn(SquareMap squareMap, int nAttackers)
    {
        int height = (int)squareMap.Height;
        int width = (int)squareMap.Width;

        List<Vector3I> validSpawns = [];
        // Side spawns
        for (int y = 0; y <= height; y++)
        {
            foreach (int x in (int[])[0, width])
            {
                Vector3I pos = new(x, 0, y);
                if (!ValidSpawn(squareMap, pos))
                    continue;
                validSpawns.Add(pos);
            }
        }
        // Top and bottom spawns
        for (int x = 0; x <= width; x++)
        {
            foreach (int y in (int[])[1, height - 1])
            {
                Vector3I pos = new(x, 0, y);
                if (!ValidSpawn(squareMap, pos))
                    continue;
                validSpawns.Add(pos);
            }
        }
        validSpawns = Shuffler.Shuffle(_random, validSpawns);
        return [.. validSpawns.Take(nAttackers)];
    }

    private bool ValidSpawn(SquareMap mapSpec, Vector3I pos)
    {
        if (IsOnTarget(mapSpec, pos) || !IsInBounds(mapSpec, pos) || IsOnObject(mapSpec, pos))
            return false;
        return true;
    }

    public bool IsInBounds(MapSpec mapSpec, Vector3I position)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot check position."),
            MapSpec.MapOneofOneofCase.SquareMap => IsInBounds(mapSpec.SquareMap, position),

            _ => throw new Exception($"Did not recognize Map type: {mapSpec.MapOneofCase}"),
        };
    }

    public bool IsOnTarget(MapSpec mapSpec, Vector3I position)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot check position."),
            MapSpec.MapOneofOneofCase.SquareMap => IsOnTarget(mapSpec.SquareMap, position),

            _ => throw new Exception($"Did not recognize Map type: {mapSpec.MapOneofCase}"),
        };
    }

    private bool IsOnTarget(SquareMap mapSpec, Vector3I position)
    {
        return position.X == mapSpec.TargetX && position.Z == mapSpec.TargetY;
    }

    private bool IsInBounds(SquareMap mapSpec, Vector3I position)
    {
        return position.X >= 0 && position.Z >= 0 && position.X <= mapSpec.Width && position.Z <= mapSpec.Height;
    }

    public List<Vector3I> GetTargetPositions(MapSpec mapSpec)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot get target positions."),
            MapSpec.MapOneofOneofCase.SquareMap => GetTargetPositions(mapSpec.SquareMap),

            _ => throw new Exception($"Did not recognize Map type: {mapSpec.MapOneofCase}"),
        };
    }

    public List<Vector3I> GetTargetPositions(SquareMap mapSpec)
    {
        // Same height as drones are spawned in
        return [new((int)mapSpec.TargetX, 1, (int)mapSpec.TargetY)];
    }

    public bool IsOnObject(MapSpec mapSpec, Vector3I position)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => IsOnObject(mapSpec.SquareMap, position),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Got MapSpec None"),
            _ => throw new Exception($"Did not recognize MapSpec: {mapSpec.MapOneofCase}"),
        };
        throw new NotImplementedException();
    }

    private bool IsOnObject(SquareMap mapSpec, Vector3I position)
    {
        var positionSet = mapSpec
            .Objects
            .Select(e => e.GetPosition())
            .Select(e => new Position(e.X, e.Y, e.Z))
            .ToHashSet();
        var p = new Position(position.X, position.Y, position.Z);

        return positionSet.Contains(p);
    }

    // Introduce record for equality-based comparison
    private record Position(int X, int Y, int Z);
}
