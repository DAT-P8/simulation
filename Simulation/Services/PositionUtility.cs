using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GW2D.V1;
using Serilog;

namespace Simulation.Services;

public class PositionUtility(Random random, ILogger logger) : IPositionUtility
{
    private readonly Random _random = random;
    private readonly ILogger _logger = logger;

    public List<Vector3I> GetSpawnPositions(MapSpec mapSpec, int count, bool isAttacker)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => GetSpawnPositions(mapSpec.SquareMap, count, isAttacker),
            MapSpec.MapOneofOneofCase.None => throw new Exception("Map is None, cannot generate position(s)."),
            _ => throw new Exception($"Did not recognize map type: {mapSpec.MapOneofCase}"),
        };
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
        return position.X >= 0 && position.Z >= 0 && position.X < mapSpec.Width && position.Z < mapSpec.Height;
    }

    private Vector3I GetSquareMapAttackerSpawn(SquareMap squareMap)
    {
        var randQuadrant = _random.Next(0, 4);

        // Left
        if (randQuadrant <= 0)
        {
            var y = _random.Next(0, (int)squareMap.Height);
            return new(0, 0, y);
        }
        // Right
        else if (randQuadrant <= 1)
        {
            var y = _random.Next(0, (int)squareMap.Height);
            return new((int)squareMap.Width - 1, 0, y);
        }
        // Down
        else if (randQuadrant <= 2)
        {
            var x = _random.Next(0, (int)squareMap.Width);
            return new(x, 0, 0);
        }
        // Down
        else
        {
            var x = _random.Next(0, (int)squareMap.Width);
            return new(x, 0, (int)squareMap.Height - 1);
        }
    }

    private Vector3I GetSquareMapDefenderSpawn(SquareMap squareMap, int radius)
    {
        var randX = _random.Next(-radius, radius + 1);
        var randY = _random.Next(-radius, radius + 1);
        return new Vector3I(randX, 0, randY) + new Vector3I((int)squareMap.TargetX, 0, (int)squareMap.TargetY);
    }

    private List<Vector3I> GetSpawnPositions(SquareMap squareMap, int count, bool isAttacker)
    {
        HashSet<Position> positions = [];

        if (isAttacker)
        {
            while (positions.Count < count)
            {
                var pos = GetSquareMapAttackerSpawn(squareMap);

                if (IsOnTarget(squareMap, pos))
                    continue;

                if (!IsInBounds(squareMap, pos))
                    continue;

                positions.Add(new(pos.X, pos.Y, pos.Z));
            }
        }
        else
        {
            // Make sure radius is big enough to spawn the amount of defenders
            var maxRadius = (int)Math.Ceiling(Math.Sqrt(positions.Count)) + 2;

            while (positions.Count < count)
            {
                var pos = GetSquareMapDefenderSpawn(squareMap, maxRadius);

                if (IsOnTarget(squareMap, pos))
                    continue;

                if (!IsInBounds(squareMap, pos))
                    continue;

                positions.Add(new(pos.X, pos.Y, pos.Z));
            }
        }

        return [.. positions.Select(e => new Vector3I(e.X, e.Y, e.Z))];
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

    // Introduce record for equality-based comparison
    private record Position(int X, int Y, int Z);
}
