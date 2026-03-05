using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Simulation.Lib.TDF;
using Simulation.Utils;
using TDFSimulation;

namespace Simulation.TDF;

public class TDFSimulation(int evaders, int pursuers, float domeRadius, int seed) : ITDFSimulation
{
    private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly PackedScene _droneEvaderScene = GD.Load<PackedScene>("res://gw_drone_evader.tscn");
    private readonly int _evaderCount = evaders;
    private readonly int _pursuerCount = pursuers;
    private readonly float _domeRadius = domeRadius;
    private readonly Random _random = new(seed);

    public Task Close()
    {
        throw new NotImplementedException();
    }

    public Task<TDFState> DoStep(List<TDFDroneAction> actions)
    {
        throw new NotImplementedException();
    }

    public Task<TDFState> New()
    {
        throw new NotImplementedException();
    }

    public Task<TDFState> Reset()
    {
        throw new NotImplementedException();
    }

    private void SpawnDrones()
    {
        List<TDFDrone> defenders = new(_pursuerCount);
        List<TDFDrone> attackers = new(_evaderCount);
        for (var i = 0; i < _pursuerCount; i++)
            defenders.Add(new TDFDrone(_droneScene.Instantiate<StaticBody3D>(), i));
        for (var i = _pursuerCount; i < _pursuerCount + _evaderCount; i++)
            attackers.Add(new TDFDrone(_droneEvaderScene.Instantiate<StaticBody3D>(), i));
    }

    private List<Vector3D<float>> RandomPositionsOnDomeAboveGround(int count)
    {
        List<Vector3D<float>> positions = new(count);
        for (var i = 0; i < count; i++)
            positions.Add(GetRandomPositionOnDomeAboveGround());

        var s = positions.First(v1 => positions.Any(v2 => v1));

        throw new NotImplementedException();
    }

    private Vector3D<float> GetRandomPositionOnDomeAboveGround()
    {
        // floats in the range ]-.5;.5]
        var randX = _random.NextSingle() - .5;
        var randZ = _random.NextSingle() - .5;
        var randY = _random.NextSingle() - .5;
        while (randY < 0)
            randY = _random.NextSingle() - .5;

        var normalized = new Vector3D<double>(randX, randY, randZ).Normalize();
        return new Vector3D<double>(normalized.X * _domeRadius, normalized.Y * _domeRadius, normalized.Z * _domeRadius).AsFloat();
    }
}
