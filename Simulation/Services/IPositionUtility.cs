using System.Collections.Generic;
using Godot;
using GW2D.V1;

namespace Simulation.Services;

public interface IPositionUtility
{
    List<Vector3I> GetSpawnPositions(MapSpec mapSpec, int count, bool isAttacker);
    bool IsInBounds(MapSpec mapSpec, Vector3I position);
    bool IsOnTarget(MapSpec mapSpec, Vector3I position);
    List<Vector3I> GetTargetPositions(MapSpec mapSpec);
}
