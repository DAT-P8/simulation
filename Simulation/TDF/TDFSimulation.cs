using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using Godot;
using Serilog;
using Simulation.Lib.TDF;
using Simulation.Utils;
using TDFSimulation;

namespace Simulation.TDF;

public class TDFSimulation(ILogger logger, long id, int evaders, int pursuers, float attackerDomeRadius, float defenderDomeRadius, float arenaDomeRadius, float maxDroneSpeed, int seed) : ITDFSimulation
{
    private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly PackedScene _droneEvaderScene = GD.Load<PackedScene>("res://gw_drone_evader.tscn");

    private readonly ILogger _logger = logger;
    private readonly long _id = id;
    private bool _terminated = false;
    private readonly int _evaderCount = evaders;
    private readonly int _pursuerCount = pursuers;
    private readonly float _attackerDomeRadius = attackerDomeRadius;
    private readonly float _defenderDomeRadius = defenderDomeRadius;
    private readonly float _arenaDomeRadius = arenaDomeRadius;
    private readonly float _maxDroneSpeed = maxDroneSpeed;
    private const float COLLISSION_RANGE = 1;
    private readonly Random _random = new(seed);

    private readonly List<TDFDrone> _defenders = [];
    private readonly List<TDFDrone> _attackers = [];

    public Task Close()
    {
        foreach (var drone in _defenders.Concat(_attackers))
            drone.Dispose();

        _defenders.Clear();
        _attackers.Clear();

        return Task.CompletedTask;
    }

    public Task<TDFState> DoStep(List<TDFDroneAction> actions)
    {
        var allDrones = _attackers.Concat(_defenders).Where(e => !e.IsDestroyed).ToList();
        foreach (var action in actions)
        {
            var drone = allDrones.FirstOrDefault(e => e.Id == action.Id);
            if (drone is null) continue;

            drone.SetForce(new Vector3D<float>(action.XF, action.YF, action.ZF));
        }

        var positionsBefore = allDrones.Select(e => e.GetPosition()).ToList();
        foreach (var drone in allDrones)
            drone.AdvanceTime(1);
        var positionsAfter = allDrones.Select(e => e.GetPosition()).ToList();

        var pointCalculations = SweepTests(positionsBefore, positionsAfter);

        foreach (var (point, v1idx, v2idx) in pointCalculations)
        {
            if (point.Dot(point) <= COLLISSION_RANGE * COLLISSION_RANGE)
            {
                var d1 = allDrones[v1idx];
                var d2 = allDrones[v2idx];

                d1.IsDestroyed = true;
                d2.IsDestroyed = true;

                d1.SetVelocity(new Vector3D<float>(0, 0, 0));
                d1.SetForce(new Vector3D<float>(0, 0, 0));

                d2.SetVelocity(new Vector3D<float>(0, 0, 0));
                d2.SetForce(new Vector3D<float>(0, 0, 0));
            }
        }

        var arenaSq = _arenaDomeRadius * _arenaDomeRadius;
        foreach (var d in allDrones)
        {
            var pos = d.GetPosition();
            if (pos.Dot(pos) > arenaSq)
            {
                d.IsDestroyed = true;
                d.SetVelocity(new Vector3D<float>(0, 0, 0));
                d.SetForce(new Vector3D<float>(0, 0, 0));
            }
        }

        return Task.FromResult(GetState());
    }

    public Task<TDFState> New()
    {
        if (_defenders.Count != 0 || _attackers.Count != 0)
            throw new Exception("New was called, but it seems to have already been initialized!");

        var (defenders, attackers) = SpawnDrones();

        _defenders.AddRange(defenders);
        _attackers.AddRange(attackers);

        return Task.FromResult(GetState());
    }

    public Task<TDFState> Reset()
    {
        _terminated = false;

        var attackerPositions = RandomPositionsOnDomeAboveGround(_attackers.Count, _attackerDomeRadius);
        var defenderPositions = RandomPositionsOnDomeAboveGround(_defenders.Count, _defenderDomeRadius);

        foreach (var (drone, pos) in _attackers.Zip(attackerPositions).Concat(_defenders.Zip(defenderPositions)))
        {
            drone.SetPosition(pos);
            drone.SetVelocity(new Vector3D<float>(0, 0, 0));
            drone.IsDestroyed = false;
        }


        return Task.FromResult(GetState());
    }

    private (List<TDFDrone>, List<TDFDrone>) SpawnDrones()
    {
        List<TDFDrone> defenders = new(_pursuerCount);
        List<TDFDrone> attackers = new(_evaderCount);

        for (var i = 0; i < _pursuerCount; i++)
            defenders.Add(new TDFDrone(_droneScene.Instantiate<StaticBody3D>(), i, false, _maxDroneSpeed));
        for (var i = _pursuerCount; i < _pursuerCount + _evaderCount; i++)
            attackers.Add(new TDFDrone(_droneEvaderScene.Instantiate<StaticBody3D>(), i, true, _maxDroneSpeed));

        foreach (var d in defenders.Concat(attackers))
            Main.MainScene.CallDeferred(Node.MethodName.AddChild, d.Body);

        var attackerPositions = RandomPositionsOnDomeAboveGround(attackers.Count, _attackerDomeRadius);
        var defenderPositions = RandomPositionsOnDomeAboveGround(defenders.Count, _defenderDomeRadius);

        foreach (var (drone, position) in attackers.Zip(attackerPositions).Concat(defenders.Zip(defenderPositions)))
            drone.SetPosition(position);

        return (defenders, attackers);
    }

    private List<Vector3D<float>> RandomPositionsOnDomeAboveGround(int count, float domeRadius)
    {
        List<Vector3D<float>> positions = new(count);
        for (var i = 0; i < count; i++)
            positions.Add(GetRandomPositionOnDomeAboveGround(domeRadius));

        // Remove positions above 0 in Y axis and remove positions that are the same.
        while (true)
        {
            List<int> idsToRemove = [];
            for (var i = 0; i < count; i++)
            {
                var v1 = positions[i];

                if (v1.Y <= 0)
                {
                    idsToRemove.Add(i);
                    continue;
                }

                for (var j = i + 1; j < count; j++)
                {
                    var v2 = positions[j];

                    if (v1.EqualsWithEpsilon(v2, 2f))
                    {
                        idsToRemove.Add(i);
                        break;
                    }
                }
            }

            if (idsToRemove.Count == 0) break;

            for (var i = idsToRemove.Count - 1; i >= 0; i--)
                positions.RemoveAt(idsToRemove[i]);

            for (var i = 0; i < idsToRemove.Count; i++)
                positions.Add(GetRandomPositionOnDomeAboveGround(domeRadius));
        }

        return positions;
    }

    private Vector3D<float> GetRandomPositionOnDomeAboveGround(float domeRadius)
    {
        // floats in the range ]-.5;.5]
        var randX = _random.NextSingle() - .5;
        var randZ = _random.NextSingle() - .5;
        var randY = _random.NextSingle() - .5;
        while (randY < 0)
            randY = _random.NextSingle() - .5;

        var normalized = new Vector3D<double>(randX, randY, randZ).Normalize();
        return new Vector3D<double>(normalized.X * domeRadius, normalized.Y * domeRadius, normalized.Z * domeRadius).AsFloat();
    }

    private TDFState GetState()
    {
        return new TDFState
        {
            SimId = _id,
            Terminated = _terminated,
            DroneStates = { _defenders.Concat(_attackers).Select(e => e.GetState()).ToList() },
        };
    }

    public static List<(Vector3D<float>, int, int)> SweepTests(List<Vector3D<float>> before, List<Vector3D<float>> after)
    {
        var orderedPairs = before.Zip(after).Select((e, i) =>
                e.First.X < e.Second.X ?
                    (e.First.X, e.Second.X) :
                    (e.Second.X, e.First.X)
            ).ToList();

        List<HashSet<int>> overlapIndeces = [];

        for (int i = 0; i < orderedPairs.Count; i++)
        {
            HashSet<int> set = [i];
            var p1 = orderedPairs[i];
            for (int j = i + 1; j < orderedPairs.Count; j++)
            {
                var p2 = orderedPairs[j];
                if (
                    (p1.Item1 <= p2.Item1 && p2.Item1 <= p1.Item2) ||
                    (p1.Item1 <= p2.Item2 && p2.Item2 <= p1.Item2) ||

                    (p2.Item1 <= p1.Item1 && p1.Item1 <= p2.Item2) ||
                    (p2.Item1 <= p1.Item2 && p1.Item2 <= p2.Item2)
                )
                {
                    set.Add(j);
                }
            }

            // Add only unique sets
            var wasFound = false;
            foreach (var s in overlapIndeces)
            {
                wasFound = wasFound || s.SetEquals(set);
                if (wasFound) break;
            }

            if (!wasFound)
                overlapIndeces.Add(set);
        }

        // Remove any subsets thay may be found.
        var overlaps = overlapIndeces.Where(s1 => overlapIndeces.All(s2 => !s1.IsProperSubsetOf(s2))).ToList();

        // Now that all overlaps in 1D have been found, we need to check if they actually did collide.
        // Start by construct the pairs to check
        List<(int, int)> overlapPairs = [];
        foreach (var overlap in overlaps)
        {
            var overlapList = overlap.ToList();
            for (int i = 0; i < overlapList.Count; i++)
            {
                for (int j = i + 1; j < overlapList.Count; j++)
                {
                    overlapPairs.Add((overlapList[i], overlapList[j]));
                }
            }
        }

        List<(Vector3D<float>, int, int)> points = [];
        foreach (var (i, j) in overlapPairs)
        {
            var v1b = before[i];
            var v1a = after[i];

            var v2b = before[j];
            var v2a = after[j];

            // Movement vectors
            var v1Mov = v1a.Sub(v1b);
            var v2Mov = v2a.Sub(v2b);
            var deltaMov = v1Mov.Sub(v2Mov);

            var P = new Vector3D<float>(0, 0, 0);
            var A = v1b.Sub(v2b);
            var B = A.Add(deltaMov);
            var point = ProjectPointOntoSegment(P, A, B);

            points.Add((point, i, j));
        }

        return points;
    }

    /*
     * <summary>
     * Project a point P onto a line segment AB
     * </summary>
     */
    private static Vector3D<float> ProjectPointOntoSegment(Vector3D<float> P, Vector3D<float> A, Vector3D<float> B)
    {
        Vector3D<float> d = B.Sub(A);
        float dDotD = d.Dot(d);

        if (dDotD == 0f)
            return A;

        float t = Math.Clamp(P.Sub(A).Dot(d) / dDotD, 0f, 1f);
        return A.Add(d.Scale(t));
    }
}
