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

public class TDFSimulation(long id, int evaders, int pursuers, float attackerDomeRadius, float defenderDomeRadius, int seed) : ITDFSimulation
{
    private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly PackedScene _droneEvaderScene = GD.Load<PackedScene>("res://gw_drone_evader.tscn");

    private readonly long _id = id;
    private bool _terminated = false;
    private readonly int _evaderCount = evaders;
    private readonly int _pursuerCount = pursuers;
    private readonly float _attackerDomeRadius = attackerDomeRadius;
    private readonly float _defenderDomeRadius = defenderDomeRadius;
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
        var allDrones = _attackers.Concat(_defenders);
        foreach (var action in actions)
        {
            var drone = allDrones.First(e => e.Id == action.Id);
            if (drone is null) continue;

            drone.SetForce(new Vector3D<float>(action.XF, action.YF, action.ZF));
        }

        foreach (var drone in allDrones)
            drone.AdvanceTime(1);

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
            defenders.Add(new TDFDrone(_droneScene.Instantiate<StaticBody3D>(), i, false));
        for (var i = _pursuerCount; i < _pursuerCount + _evaderCount; i++)
            attackers.Add(new TDFDrone(_droneEvaderScene.Instantiate<StaticBody3D>(), i, true));

        foreach (var d in defenders.Concat(attackers))
        {
            Main.Scene.CallDeferred(Node.MethodName.AddChild, d.Body);
        }

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

                    // This check needs to be even more loose!
                    if (v1.EqualsWithEpsilon(v2))
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
}
