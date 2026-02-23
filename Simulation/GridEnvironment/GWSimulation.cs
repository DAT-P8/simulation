using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using GWSimulation;
using Serilog;
using Simulation.Lib;

namespace Simulation.GridEnvironment;

public class GWSim : IGWSimulation
{
	private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");

    private readonly Dictionary<long, GWDrone> _defenderDrones = [];
    private readonly Dictionary<long, GWDrone> _evaderDrones = [];
    private readonly Lock _droneLock = new();


    public Task Close()
    {
        DisposeDrones();
        return Task.CompletedTask;
    }

    public Task<GWState> DoStep(List<GWDroneAction> actions)
    {
        foreach (var action in actions)
        {
            // TODO: Basically just missing some AAAAACTIONNNNNNNSSS?
        }

        throw new System.NotImplementedException();
    }

    public Task<GWState> Reset()
    {
        DisposeDrones();

        var defender_drone_1 = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), 0);
        var defender_drone_2 = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), 1);
        var evader_drone = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), 2);

        // We just don't ever set Y
        // Position are 0 < 11
        // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        // defended area is in (5, 5)
        

        defender_drone_1.X = 6;
        defender_drone_1.Y = 0;
        defender_drone_1.Z = 5;

        defender_drone_2.X = 5;
        defender_drone_2.Y = 0;
        defender_drone_2.Z = 6;

        evader_drone.X = 0;
        evader_drone.Y = 0;
        evader_drone.Z = 0;

        Main.Scene.CallDeferred(Node.MethodName.AddChild, defender_drone_1.StaticBody3D);
        Main.Scene.CallDeferred(Node.MethodName.AddChild, defender_drone_2.StaticBody3D);
        Main.Scene.CallDeferred(Node.MethodName.AddChild, evader_drone.StaticBody3D);

        var state = new GWState {
            DefenderDroneStates = {
                defender_drone_1.GetState(),
                defender_drone_2.GetState()
            },
            EvaderDroneStates = {
                evader_drone.GetState()
            },
            Terminated = false
        };

        return Task.FromResult(state);
    }

    private void DisposeDrones()
    {
        lock (_droneLock)
        {
            foreach (var (_, d) in _defenderDrones)
                d.StaticBody3D.QueueFree();
            foreach (var (_, d) in _evaderDrones)
                d.StaticBody3D.QueueFree();

            _defenderDrones.Clear();
            _evaderDrones.Clear();
        }
    }
}
