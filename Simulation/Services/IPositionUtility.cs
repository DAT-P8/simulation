using System.Collections.Generic;
using Godot;
using GW2D.V1;

namespace Simulation.Services;

public interface IPositionUtility
{
    List<Vector2I> GetPursuerSpawn(MapSpec mapSpec, int count);
    List<Vector2I> GetEvaderSpawn(MapSpec mapSpec, int count);
    bool IsInBounds(MapSpec mapSpec, Vector2I position);
    bool IsOnTarget(MapSpec mapSpec, Vector2I position);
    bool IsOnObject(MapSpec mapSpec, Vector2I position);
    List<Vector2I> GetTargetPositions(MapSpec mapSpec);
}
