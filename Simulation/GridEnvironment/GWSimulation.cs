using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using GWSimulation;
using Serilog;
using Simulation.Lib;

namespace Simulation.GridEnvironment;

public class GWSim(ILogger logger) : IGWSimulation
{
    private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly ILogger _logger = logger;
    private readonly Dictionary<long, GWDrone> _drones = [];
    private readonly Lock _droneLock = new();

    private bool _isTerminated = false;


    public Task Close()
    {
        _logger.Information("GWSim Close started");
        DisposeDrones();
        _logger.Information("GWSim Close done");
        return Task.CompletedTask;
    }

    public Task<GWState> DoStep(List<GWDroneAction> actions)
    {
        _logger.Information("GWSim DoStep started");
        if (_isTerminated) 
        {
            _logger.Information("GWSim DoStep done");
            return Task.FromResult(GetState());
        }

        foreach (var droneAction in actions)
        {
            var id = droneAction.Id;
            var action = droneAction.Action;

            if (!_drones.TryGetValue(id, out var drone))
            {
                _logger.Error("Did not find a drone with id {Id}", id);
                continue;
            }

            if (drone.Destroyed) continue;

            var x_diff = action switch {
                GWAction.Left => -1,
                GWAction.Right => 1,
                _ => 0,
            };
            var z_diff = action switch {
                GWAction.Up => 1,
                GWAction.Down => -1,
                _ => 0,
            };

            if (x_diff == 0 && z_diff == 0 && action != GWAction.Nothing)
            {
                _logger.Error("Did not recognize action {Action}", action);
                continue;
            }

            // Checks if the new position is out of bounds, if so => kill it
            var newX = drone.X + x_diff;
            var newZ = drone.Z + z_diff;

            if (!IsInBounds(newX, newZ))
                drone.Destroyed = true;

            // Defender drones are not allowed in the target area.
            if (!drone.IsEvader && newX == 5 && newZ == 5)
                continue;

            drone.X = newX;
            drone.Z = newZ;
        }

        bool targetReached = _drones.Any(e => e.Value.X == 5 && e.Value.Z == 5 && e.Value.IsEvader);
        bool evadersFled = _drones.All(e => !e.Value.IsEvader || IsInBounds(e.Value.X, e.Value.Z));
        bool evadersDied = _drones.All(e => !e.Value.IsEvader || e.Value.Destroyed);

        if (targetReached || evadersFled || evadersDied)
            _isTerminated = true;

        _logger.Information("GWSim DoStep done");
        return Task.FromResult(GetState());
    }

    public Task<GWState> Reset()
    {
        _logger.Information("GWSim Reset started");
        DisposeDrones();

        var drones = GetInitialDrones();

        foreach (var d in drones)
            Main.Scene.CallDeferred(Node.MethodName.AddChild, d.StaticBody3D);

        lock (_droneLock)
        {
            foreach (var d in drones)
                _drones.Add(d.Id, d);
        }

        _logger.Information("GWSim Reset done");
        return Task.FromResult(GetState());
    }

    private List<GWDrone> GetInitialDrones()
    {
        var defender_drone_1 = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), 0, false);
        var defender_drone_2 = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), 1, false);
        var evader_drone = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), 2, true);

        // TODO: Randomize initial positions!
        Random random = new();

        // Lower quadrant or upper quadrant
        var lower = (1 & random.Next()) == 1;
        var left = (1 & random.Next()) == 1;
        var rand = random.Next(0, 11);


        defender_drone_1.X = 6;
        defender_drone_1.Y = 0;
        defender_drone_1.Z = 5;

        defender_drone_2.X = 5;
        defender_drone_2.Y = 0;
        defender_drone_2.Z = 6;

        evader_drone.Y = 0;
        if (lower)
        {
            evader_drone.X = !left ? rand : 0;
            evader_drone.Z = left ? rand: 0;
        }
        else
        {
            evader_drone.X = !left ? rand : 10;
            evader_drone.Z = !left ? rand : 10;
        }

        return [defender_drone_1, defender_drone_2, evader_drone];
    }

    private GWState GetState()
    {
        return new GWState {
            DroneStates = { _drones.Select(e => e.Value.GetState())},
            Terminated = _isTerminated,
        };
    }

    private void DisposeDrones()
    {
        lock (_droneLock)
        {
            foreach (var (_, d) in _drones)
                d.StaticBody3D.QueueFree();

            _drones.Clear();
        }
    }

    private static bool IsInBounds(int x, int z)
    {
        return 0 <= x && x <= 10 && 0 <= z && z <= 10;
    }
}
