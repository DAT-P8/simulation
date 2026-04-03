using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GW2D.V1;
using Simulation.Instances;
using Simulation.Lib.GW;
using Simulation.Services;

namespace Simulation.GW;

public class GWSimulation(IDroneSpawner droneSpawner, IMapSpawner mapSpawner, IPositionUtility positionUtility, long id) : IGWSimulation, IDisposable
{
    private GWSimulationInstance? _instance = null;
    private readonly IDroneSpawner _droneSpawner = droneSpawner;
    private readonly IMapSpawner _mapSpawner = mapSpawner;
    private readonly IPositionUtility _positionUtility = positionUtility;
    private readonly long _id = id;

    public async Task Close()
    {
        if (_instance is null)
            throw new Exception("Attempted to close a non-existing simulation!");

        await _instance.Close();

        _instance?.Dispose();
        _instance = null;
    }

    public async Task<State> DoStep(List<DroneAction> actions)
    {
        if (_instance is null)
            throw new Exception("Attempted to do step of uninitialized simulation");

        var state = await _instance.DoStep(actions);

        return state;
    }

    public async Task<State> New(MapSpec mapSpec, int evaders, int pursuers, int drone_velocity)
    {
        if (_instance is not null)
            throw new Exception("Attempted override an existing simulation!");

        var (state, newInstance) = await CreateNewSimulation(pursuers, evaders, drone_velocity, mapSpec);
        _instance = newInstance;

        return state;
    }

    public async Task<State> Reset()
    {
        if (_instance is null)
            throw new Exception("Attempted to reset an uninitialized simulation");

        return await _instance.Reset();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // Dispose instance if applicable
        _instance?.Dispose();
    }

    private async Task<(State, GWSimulationInstance)> CreateNewSimulation(int defenders, int attackers, int velocity, MapSpec mapSpec)
    {
        List<GWDrone> defenderDrones = new(defenders);
        List<GWDrone> attackerDrones = new(attackers);
        for (int i = 0; i < defenders; i++)
        {
            var d = _droneSpawner.SpawnDrone(i, velocity, false);
            defenderDrones.Add(d);
        }

        for (int i = defenders; i < attackers + defenders; i++)
        {
            var a = _droneSpawner.SpawnDrone(i, velocity, true);
            attackerDrones.Add(a);
        }

        _mapSpawner.SpawnMap(mapSpec);

        var sim = new GWSimulationInstance(defenderDrones, attackerDrones, _positionUtility, mapSpec, _id);
        var state = await sim.Reset();

        return (state, sim);
    }
}

public class GWSimulationInstance(
    List<GWDrone> defenders,
    List<GWDrone> attackers,
    IPositionUtility positionUtility,
    MapSpec mapSpec,
    long simId
) : IDisposable
{
    private readonly List<GWDrone> _defenders = defenders;
    private readonly List<GWDrone> _attackers = attackers;
    private readonly IPositionUtility _positionUtility = positionUtility;
    private readonly MapSpec _mapSpec = mapSpec;
    private readonly long _simId = simId;

    public Task Close()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public Task<State> DoStep(List<DroneAction> actions)
    {
        var allDrones = _defenders.Concat(_attackers).ToList();

        foreach (var action in actions)
        {
            var drone = allDrones.FirstOrDefault(d => d.Id == action.Id) ??
                throw new Exception($"Did not find a drone with id: {action.Id}!");

            if (drone.Destroyed) continue;

            var vec = DtoMapper.ToVector(action.Action) * (int)action.Velocity;
            drone.SetPosition(drone.GetPosition() + vec);
        }


        return Task.FromResult(GetState());
    }

    public Task<State> Reset()
    {
        var defPositions = _positionUtility.GetSpawnPositions(_mapSpec, _defenders.Count, false);
        var attPositions = _positionUtility.GetSpawnPositions(_mapSpec, _attackers.Count, false);

        foreach (var (drone, pos) in _defenders.Zip(defPositions))
            drone.SetPosition(new GWPosition(pos.X, pos.Y, pos.Z));
        foreach (var (drone, pos) in _attackers.Zip(attPositions))
            drone.SetPosition(new GWPosition(pos.X, pos.Y, pos.Z));

        foreach (var drone in _attackers.Concat(_defenders))
            drone.Destroyed = false;

        return Task.FromResult(GetState());
    }

    public State GetState()
    {
        State state = new()
        {
            Events = {},
            Terminated = false,
            SimId = _simId,
            DroneStates = {
                _attackers.Concat(_defenders).Select(e => new DroneState {
                    Destroyed = false,
                    X = e.X,
                    Y = e.Y,
                    Id = e.Id,
                    IsEvader = e.IsEvader
                })
            },
        };

        return state;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var d in _defenders.Concat(_attackers))
            d.StaticBody3D.QueueFree();

        _defenders.Clear();
        _attackers.Clear();
    }
}
