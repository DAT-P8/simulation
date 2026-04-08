using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GW2D.V1;
using Serilog;
using Simulation.Instances;
using Simulation.Lib.GW;
using Simulation.Services;
using Simulation.Utils;

namespace Simulation.GW;

public class GWSimulation(ILogger logger, IDroneSpawner droneSpawner, IMapSpawner mapSpawner, IPositionUtility positionUtility, long id) : IGWSimulation, IDisposable
{
    private GWSimulationInstance? _instance = null;
    private readonly ILogger _logger = logger;
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

        var sim = new GWSimulationInstance(defenderDrones, attackerDrones, _positionUtility, mapSpec, _logger, _id);
        var state = await sim.Reset();

        return (state, sim);
    }
}

public class GWSimulationInstance(
    List<GWDrone> defenders,
    List<GWDrone> attackers,
    IPositionUtility positionUtility,
    MapSpec mapSpec,
    ILogger logger,
    long simId
) : IDisposable
{
    private readonly List<GWDrone> _defenders = defenders;
    private readonly List<GWDrone> _attackers = attackers;
    private readonly IPositionUtility _positionUtility = positionUtility;
    private readonly MapSpec _mapSpec = mapSpec;
    private readonly ILogger _logger = logger;
    private readonly long _simId = simId;

    public Task Close()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public Task<State> DoStep(List<DroneAction> actions)
    {
        var allDrones = _defenders.Concat(_attackers).ToList();

        var nonDestroyedDrones = allDrones.Where(e => !e.Destroyed).ToList();
        // Before positions of non-destroyed drones
        var beforePositions = nonDestroyedDrones.Select(e => e.GetPosition()).ToList();

        // Move non-destroyed drones
        foreach (var action in actions)
        {
            var drone = allDrones.FirstOrDefault(d => d.Id == action.Id) ??
                throw new Exception($"Did not find a drone with id: {action.Id}!");

            if (drone.Destroyed) continue;

            var vec = DtoMapper.ToVector(action.Action) * (int)action.Velocity;
            _logger.Information("New position: {NP}", drone.GetPosition() + vec);

            drone.SetPosition(drone.GetPosition() + vec);
        }

        // After positions of non-destroyed drones
        var afterPositions = nonDestroyedDrones.Select(e => e.GetPosition()).ToList();

        var possibleCollisions = VectorExtensions.SweepTests(
            [.. beforePositions.Select(e => new Vector3D<float>(e.X, e.Y, e.Z))],
            [.. afterPositions.Select(e => new Vector3D<float>(e.X, e.Y, e.Z))]
        );

        List<(GWDrone, GWDrone)> collisions = [];
        foreach (var col in possibleCollisions)
        {
            var (distanceVector, idx1, idx2) = col;

            var distance = distanceVector.Dot(distanceVector);
            if (distance > 1)
                continue;

            var d1 = nonDestroyedDrones[idx1];
            var d2 = nonDestroyedDrones[idx2];

            d1.Destroyed = true;
            d2.Destroyed = true;

            collisions.Add((nonDestroyedDrones[idx1], nonDestroyedDrones[idx2]));
        }
        var collisionEvents = collisions.Select(e => new CollisionEvent { DroneIds = { e.Item1.Id, e.Item2.Id } }).ToList();

        var outOfBoundsDrones = nonDestroyedDrones
            .Where(e => !_positionUtility.IsInBounds(_mapSpec, e.GetPosition()))
            .ToList();

        foreach (var d in outOfBoundsDrones)
            d.Destroyed = true;

        // Filter away drones that have crashed from the out of bounds events
        var outOfBoundsIds = outOfBoundsDrones
            .Where(outDrone => !collisionEvents.Any(e => e.DroneIds.Contains(outDrone.Id)))
            .Select(e => e.Id)
            .ToList();

        var outOfBoundsEvent = new OutOfBoundsEvent
        {
            DroneIds = { outOfBoundsIds }
        };

        List<Event> events = [
            .. collisionEvents
                .Select(e => new Event { CollisionEvent = e }),
            new Event { OutOfBoundsEvent = outOfBoundsEvent }
        ];

        return Task.FromResult(GetState(events));
    }

    public Task<State> Reset()
    {
        var defPositions = _positionUtility.GetSpawnPositions(_mapSpec, _defenders.Count, false);
        var attPositions = _positionUtility.GetSpawnPositions(_mapSpec, _attackers.Count, false);

        foreach (var (drone, pos) in _defenders.Zip(defPositions))
            drone.SetPosition(new Vector3I(pos.X, 1, pos.Z));
        foreach (var (drone, pos) in _attackers.Zip(attPositions))
            drone.SetPosition(new Vector3I(pos.X, 1, pos.Z));

        foreach (var drone in _attackers.Concat(_defenders))
            drone.Destroyed = false;

        return Task.FromResult(GetState());
    }

    public State GetState()
    {
        return GetState([]);
    }

    public State GetState(List<Event> events)
    {
        bool terminated = _defenders.Concat(_attackers).All(e => e.Destroyed);
        _logger.Information("Terminated: {T}", terminated);
        State state = new()
        {
            Events = { events },
            Terminated = terminated,
            SimId = _simId,
            DroneStates = {
                _attackers.Concat(_defenders).Select(e => new DroneState {
                    Destroyed = e.Destroyed,
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

    private record PositionID(Vector3I Position, long Id);
    private record PositionPairId(Vector3I Before, Vector3I After, long Id);
}
