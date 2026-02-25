using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GWSimulation;
using Serilog;
using Simulation.Lib;

namespace Simulation.GridEnvironment;

public class GWSim(ILogger logger) : IGWSimulation
{
    private readonly long Defender1Id = 0;
    private readonly long Defender2Id = 1;
    private readonly long EvaderId = 2;

    private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly PackedScene _droneEvaderScene = GD.Load<PackedScene>("res://gw_drone_evader.tscn");
    private readonly ILogger _logger = logger;
    private readonly Dictionary<long, GWDrone> _drones = [];
    private readonly object _droneLock = new();

    private bool _isTerminated = false;


    public Task Close()
    {
        DisposeDrones();
        return Task.CompletedTask;
    }

    public Task<GWState> DoStep(List<GWDroneAction> actions)
    {
        if (_isTerminated)
        {
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

            var x_diff = action switch
            {
                GWAction.Left => -1,
                GWAction.Right => 1,
                _ => 0,
            };
            var z_diff = action switch
            {
                GWAction.Up => 1,
                GWAction.Down => -1,
                _ => 0,
            };

            if (x_diff == 0 && z_diff == 0 && action != GWAction.Nothing)
            {
                _logger.Error("Did not recognize action {Action}", action);
                continue;
            }

            var x_pos = drone.X;
            var z_pos = drone.Z;

            // Checks if the new position is out of bounds, if so => kill it
            var newX = x_pos + x_diff;
            var newZ = z_pos + z_diff;

            if (!IsInBounds(newX, newZ))
                drone.Destroyed = true;

            // Defender drones are not allowed in the target area.
            if (!drone.IsEvader && newX == 5 && newZ == 5)
                continue;

            drone.SetPosition(new GWPosition(newX, 0, newZ));
        }

        // Any evader reaches target (5, 5)
        _isTerminated = _drones.Any(e => e.Value.X == 5 && e.Value.Z == 5 && e.Value.IsEvader);
        if (_isTerminated)
            return Task.FromResult(GetState());
        
        // All evaders are out of bounds
        _isTerminated = _drones.All(e => !e.Value.IsEvader || !IsInBounds(e.Value.X, e.Value.Z));
        if (_isTerminated)
            return Task.FromResult(GetState());

        var colliding_drones = _drones
            .Where(d1 =>
                _drones.Any(d2 =>
                    d1.Value.Id != d2.Value.Id &&
                    d1.Value.X == d2.Value.X
                    && d1.Value.Z == d2.Value.Z
                ))
            .Select(e => e.Value);

        foreach (var drone in colliding_drones)
            drone.Destroyed = true;

        // All evaders are destroyed
        _isTerminated = _drones.All(e => !e.Value.IsEvader || e.Value.Destroyed);
        return Task.FromResult(GetState());
    }

    public Task<GWState> Reset()
    {
        _isTerminated = false;

        if (_drones.Count > 0)
        {
            var defender_drone_1 = _drones[Defender1Id];
            var defender_drone_2 = _drones[Defender2Id];
            var evader_drone = _drones[EvaderId];

            var positions = GetInitialPositions();

            defender_drone_1.SetPosition(positions.Defender1);
            defender_drone_2.SetPosition(positions.Defender2);
            evader_drone.SetPosition(positions.Evader);

            defender_drone_1.Destroyed = false;
            defender_drone_2.Destroyed = false;
            evader_drone.Destroyed = false;
        }
        else
        {
            var drones = GetInitialDrones();

            foreach (var d in drones)
                Main.Scene.CallDeferred(Node.MethodName.AddChild, d.StaticBody3D);

            lock (_droneLock)
            {
                foreach (var d in drones)
                    _drones.Add(d.Id, d);
            }
        }

        return Task.FromResult(GetState());
    }

    private record Positions
    {
        public GWPosition Defender1;
        public GWPosition Defender2;
        public GWPosition Evader;
    }

    private static Positions GetInitialPositions()
    {
        Random random = new();

        var lower = (1 & random.Next()) == 1;
        var left = (1 & random.Next()) == 1;
        var rand = random.Next(0, 11);

        GWPosition defender_drone_1 = new(6, 0, 5);
        GWPosition defender_drone_2 = new(5, 0, 6);

        GWPosition evader_position;
        if (lower)
            evader_position = new(!left ? rand : 0, 0, left ? rand : 0);
        else
            evader_position = new(!left ? rand : 10, 0, left ? rand : 10);

        GWPosition evader_drone = evader_position;

        return new Positions
        {
            Defender1 = defender_drone_1,
            Defender2 = defender_drone_2,
            Evader = evader_drone
        };
    }

    private List<GWDrone> GetInitialDrones()
    {
        var defender_drone_1 = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), Defender1Id, false);
        var defender_drone_2 = new GWDrone(_droneScene.Instantiate<StaticBody3D>(), Defender2Id, false);
        var evader_drone = new GWDrone(_droneEvaderScene.Instantiate<StaticBody3D>(), EvaderId, true);

        var positions = GetInitialPositions();
        defender_drone_1.SetPosition(positions.Defender1);
        defender_drone_2.SetPosition(positions.Defender2);
        evader_drone.SetPosition(positions.Evader);

        return [defender_drone_1, defender_drone_2, evader_drone];
    }

    private GWState GetState()
    {
        return new GWState
        {
            DroneStates = { _drones.Select(e => e.Value.GetState()) },
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
