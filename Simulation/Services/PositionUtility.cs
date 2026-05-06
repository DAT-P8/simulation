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

    public List<Vector2I> GetPursuerSpawn(MapSpec mapSpec, int count)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => GetPursuerSpawn(mapSpec.SquareMap, count),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot generate position(s)."),
            _ => throw new Exception($"Did not recognize map type: {mapSpec.MapOneofCase}"),
        };
    }

    public List<Vector2I> GetPursuerSpawn(SquareMap squareMap, int nDefenders)
    {
        // Make sure radius is big enough to spawn the amount of defenders
        int maxRadius = (int)Math.Ceiling(Math.Sqrt(nDefenders)) + 2;
        Vector2I targetPos = new((int)squareMap.TargetX, (int)squareMap.TargetY);

        List<Vector2I> validSpawns = [];
        for (int i = -maxRadius; i <= maxRadius; i++)
        {
            for (int j = -maxRadius; j <= maxRadius; j++)
            {
                Vector2I pos = new Vector2I(i, j) + targetPos;
                if (!ValidSpawn(squareMap, pos))
                    continue;
                validSpawns.Add(pos);
            }
        }
        validSpawns = validSpawns.OrderBy(_ => _random.Next()).ToList();
        return [.. validSpawns.Take(nDefenders)];
    }

    public List<Vector2I> GetEvaderSpawn(MapSpec mapSpec, int count)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => GetEvaderSpawn(mapSpec.SquareMap, count),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot generate position(s)."),
            _ => throw new Exception($"Did not recognize map type: {mapSpec.MapOneofCase}"),
        };
    }

    public List<Vector2I> GetEvaderSpawn(SquareMap squareMap, int nAttackers)
    {
        int height = (int)squareMap.Height - 1;
        int width = (int)squareMap.Width - 1;

        List<Vector2I> validSpawns = [];
        // Side spawns
        for (int y = 0; y < height; y++)
        {
            foreach (int x in (int[])[0, width])
            {
                Vector2I pos = new(x, y);
                if (!ValidSpawn(squareMap, pos))
                    continue;
                validSpawns.Add(pos);
            }
        }
        // Top and bottom spawns
        for (int x = 0; x < width; x++)
        {
            foreach (int y in (int[])[0, height])
            {
                Vector2I pos = new(x, y);
                if (!ValidSpawn(squareMap, pos))
                    continue;
                validSpawns.Add(pos);
            }
        }
        validSpawns = validSpawns.OrderBy(_ => _random.Next()).ToList();
        return [.. validSpawns.Take(nAttackers)];
    }

    private bool ValidSpawn(SquareMap mapSpec, Vector2I pos)
    {
        if (IsOnTarget(mapSpec, pos) || !IsInBounds(mapSpec, pos) || IsOnObject(mapSpec, pos))
            return false;
        return true;
    }

    public bool IsInBounds(MapSpec mapSpec, Vector2I position)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot check position."),
            MapSpec.MapOneofOneofCase.SquareMap => IsInBounds(mapSpec.SquareMap, position),

            _ => throw new Exception($"Did not recognize Map type: {mapSpec.MapOneofCase}"),
        };
    }

    public bool IsOnTarget(MapSpec mapSpec, Vector2I position)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot check position."),
            MapSpec.MapOneofOneofCase.SquareMap => IsOnTarget(mapSpec.SquareMap, position),

            _ => throw new Exception($"Did not recognize Map type: {mapSpec.MapOneofCase}"),
        };
    }

    private bool IsOnTarget(SquareMap mapSpec, Vector2I position)
    {
        return position.X == mapSpec.TargetX && position.Y == mapSpec.TargetY;
    }

    private bool IsInBounds(SquareMap mapSpec, Vector2I position)
    {
        return position.X >= 0 &&
                position.Y >= 0 &&
                position.X < mapSpec.Width &&
                position.Y < mapSpec.Height;
    }

    public List<Vector2I> GetTargetPositions(MapSpec mapSpec)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot get target positions."),
            MapSpec.MapOneofOneofCase.SquareMap => GetTargetPositions(mapSpec.SquareMap),

            _ => throw new Exception($"Did not recognize Map type: {mapSpec.MapOneofCase}"),
        };
    }

    public List<Vector2I> GetTargetPositions(SquareMap mapSpec)
    {
        return [new((int)mapSpec.TargetX, (int)mapSpec.TargetY)];
    }

    public bool IsOnObject(MapSpec mapSpec, Vector2I position)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => IsOnObject(mapSpec.SquareMap, position),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Got MapSpec None"),
            _ => throw new Exception($"Did not recognize MapSpec: {mapSpec.MapOneofCase}"),
        };
        throw new NotImplementedException();
    }

    private bool IsOnObject(SquareMap mapSpec, Vector2I position)
    {
        var objectPos = mapSpec.Objects.Select(e => e.GetPosition());
        return objectPos.Any(o => o == position);
    }
}
