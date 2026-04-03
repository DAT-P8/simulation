using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GW2D.V1;

namespace Simulation.Services;

public class PositionUtility(Random random) : IPositionUtility
{
    private readonly Random _random = random;

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
        return position.X == mapSpec.TargetX && position.Y == mapSpec.TargetY;
    }

    private bool IsInBounds(SquareMap mapSpec, Vector3I position)
    {
        return position.X >= 0 && position.Y >= 0 && position.X <= mapSpec.Width - 1 && position.Y <= mapSpec.Height - 1;
    }

    private Vector3I GetSquareMapAttackerSpawn(SquareMap squareMap)
    {
        var randQuadrant = _random.Next(0, 4);

        // Left
        if (randQuadrant <= 0)
        {
            var y = _random.Next(0, (int)squareMap.Height);
            return new(0, y, 0);
        }
        // Right
        else if (randQuadrant <= 1)
        {
            var y = _random.Next(0, (int)squareMap.Height);
            return new((int)squareMap.Width - 1, y, 0);
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
            return new(x, (int)squareMap.Height - 1, 0);
        }
    }
    
    private Vector3I GetSquareMapDefenderSpawn(SquareMap squareMap, int radius)
    {
        var randX = _random.Next(-radius, radius + 1);
        var randY = _random.Next(-radius, radius + 1);
        return new Vector3I(randX, randY, 0) + new Vector3I((int)squareMap.TargetX, (int)squareMap.TargetY, 0);
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
            var maxRadius = (int)Math.Ceiling(Math.Sqrt(positions.Count)) + 1;

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

    // Introduce record for equality-based comparison
    private record Position(int X, int Y, int Z);
}
