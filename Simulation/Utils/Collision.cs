using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GW2D.V1;
using Simulation.Instances;
using Simulation.Services;

namespace Simulation.Utils;

public class CollisionChecker()
{
    public static List<long> ObjectCollisions(List<(GWDrone drone, DroneAction actions)> drones, List<ObjectSpec> objects)
    {
        var droneActionData = drones.Select(e => (
                    e.drone.Id,
                    e.drone.GetPosition(),
                    DtoMapper.ToVector(e.actions.Action),
                    e.actions.Velocity)
                ).ToList();
        var objectPositions = objects.Select(e => e.GetPosition()).ToList();
        return IntersectAnyPoint(droneActionData, objectPositions);
    }

    public static List<long> TargetCollisions(List<(GWDrone drone, DroneAction actions)> drones, List<Vector2I> targetTiles)
    {
        var droneActionData = drones.Select(e => (
                    e.drone.Id,
                    e.drone.GetPosition(),
                    DtoMapper.ToVector(e.actions.Action),
                    e.actions.Velocity)
                ).ToList();
        return IntersectAnyPoint(droneActionData, targetTiles);
    }

    public static List<long> IntersectAnyPoint(
            List<(long droneId, Vector2I dronePos, Vector2I actionVec, long velocity)> drones,
            List<Vector2I> points)
    {
        List<long> intersectingDrones = [];
        foreach (var (droneId, dronePos, actionVec, velocity) in drones)
        {
            for (int v = 0; v <= velocity; v++) //v = 1 should be valid as we cannot start on an illegal square
            {
                Vector2I nextPosition = dronePos + (actionVec * v);
                if (points.Contains(nextPosition))
                {
                    intersectingDrones.Add(droneId);
                    break;
                }
            }
        }
        return intersectingDrones;
    }

    public static List<(long, long)> DroneCollisions(List<(GWDrone drone, DroneAction actionVec)> drones)
    {
        var droneActionData = drones.Select(e => (
                    e.drone.Id,
                    e.drone.GetPosition(),
                    DtoMapper.ToVector(e.actionVec))
                );
        List<(long, long)> collidingDrones = [];
        foreach (var (id1, pos1, act1) in droneActionData)
        {
            foreach (var (id2, pos2, act2) in droneActionData)
            {
                if (id1 == id2)
                    continue;
                Vector2 sweepVec = SweepPair((pos1, act1), (pos2, act2));
                if (sweepVec.Dot(sweepVec) <= .5)
                    collidingDrones.Add((id1, id2));
            }
        }
        return collidingDrones;
    }

    public static Vector2 SweepPair((Vector2I pos, Vector2I act) drone1, (Vector2I pos, Vector2I act) drone2)
    {
        Vector2 initialPos1 = drone1.pos;
        Vector2 initialPos2 = drone2.pos;

        Vector2 actionVec1 = drone1.act;
        Vector2 actionVec2 = drone2.act;
        Vector2 deltaActionVec = actionVec1 - actionVec2;

        Vector2 zeroVector = new(0, 0);
        Vector2 deltaPos = initialPos1 - initialPos2;
        Vector2 deltaPosMov = deltaPos + deltaActionVec;

        return ProjectPointOntoSegment(zeroVector, deltaPos, deltaPosMov);
    }

    private static Vector2 ProjectPointOntoSegment(Vector2 point, Vector2 vecA, Vector2 vecB)
    {
        Vector2 deltaVec = vecB - vecA;
        float dot = deltaVec.Dot(deltaVec);

        if (dot == 0f)
            return vecA;

        float t = Math.Clamp((point - vecA).Dot(deltaVec) / dot, 0f, 1f);
        Vector2 scaled = vecA + (deltaVec * t);
        return scaled;
    }
}
