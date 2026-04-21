using System;
using System.Collections.Generic;
using Godot;
using GW2D.V1;

namespace Simulation.Services;

public static class ObjectUtility
{
    public static Vector3I GetPosition(this ObjectSpec objectSpec)
    {
        return objectSpec.ObjectOneofCase switch
        {
            ObjectSpec.ObjectOneofOneofCase.None => throw new Exception("Received a None object specification"),
            ObjectSpec.ObjectOneofOneofCase.SquareObject => GetPosition(objectSpec.SquareObject),
            _ => throw new Exception($"Did not recognize one of case: {objectSpec.ObjectOneofCase}")
        };
    }

    public static Vector3I GetPosition(this SquareObject objectSpec)
    {
        return new((int)objectSpec.X, 0, (int)objectSpec.Y);
    }

    public static List<ObjectSpec> GetObjects(this MapSpec mapSpec)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => [.. mapSpec.SquareMap.Objects],
            MapSpec.MapOneofOneofCase.None => throw new Exception("MapSpec was None"),
            _ => throw new Exception($"Did not recognize MapSpec case: {mapSpec.MapOneofCase}"),
        };
    }
}
