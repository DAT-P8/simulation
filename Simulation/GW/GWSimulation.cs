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
        var beforePositions = _defenders.Concat(_attackers)
            .Where(e => !e.Destroyed)
            .Select(e => (e.Id, e.GetPosition()))
            .ToList();

        foreach (var action in actions)
        {
            var drone = allDrones.FirstOrDefault(e => e.Id == action.Id) ??
                throw new Exception($"Did not find drone with id: {action.Id}");
            if (drone.Destroyed) continue;
            var resultingPosition = drone.GetPosition() + DtoMapper.ToVector(action.Action) * (int)action.Velocity;
            drone.SetPosition(resultingPosition);
        }

        var afterPositions = _defenders.Concat(_attackers)
            .Where(e => !e.Destroyed)
            .Select(e => (e.Id, e.GetPosition()))
            .ToList();

        var zippedPositions = beforePositions
            .Zip(afterPositions)
            .Select(e => (e.First.Id, e.First.Item2, e.Second.Item2))
            .ToList();

        var collisions = GetCollisions(zippedPositions);
        var outOfBounds = GetOutOfBounds(zippedPositions);

        foreach (var collision in collisions)
        {
            foreach (var id in collision.DroneIds)
            {
                var drone = allDrones.FirstOrDefault(e => e.Id == id) ??
                    throw new Exception($"Detected a collision with a drone that doesn't exist: {id}");

                drone.Destroyed = true;
            }
        }

        foreach (var id in outOfBounds.DroneIds)
        {
            var drone = allDrones.FirstOrDefault(e => e.Id == id) ??
                throw new Exception($"Detected an out of bounds event with a drone that doesn't exist: {id}");

            drone.Destroyed = true;
        }

        var collisionEvents = collisions.Select(e => new Event { CollisionEvent = e }).ToList();
        var outOfBoundsEvent = new Event { OutOfBoundsEvent = outOfBounds };
        return Task.FromResult(GetState([.. collisionEvents, outOfBoundsEvent]));
    }

    private OutOfBoundsEvent GetOutOfBounds(List<(long Id, Vector3I, Vector3I)> positions)
    {
        List<long> ids = [];
        foreach (var position in positions)
        {
            var (id, _, after) = position;

            if (_positionUtility.IsInBounds(_mapSpec, after))
                continue;

            ids.Add(id);
        }

        return new OutOfBoundsEvent
        {
            DroneIds = { ids }
        };
    }

    private List<CollisionEvent> GetCollisions(List<(long Id, Vector3I, Vector3I)> positions)
    {
        var before = positions.Select(e => e.Item3).Select(e => new Vector3D<float>(e.X, e.Y, e.Z)).ToList();
        var after = positions.Select(e => e.Item2).Select(e => new Vector3D<float>(e.X, e.Y, e.Z)).ToList();

        var results = VectorExtensions.SweepTests(before, after);

        List<CollisionEvent> events = [];
        foreach (var result in results)
        {
            var (pointProjection, idx1, idx2) = result;
            var colEvent = new CollisionEvent
            {
                DroneIds = { idx1, idx2 }
            };

            events.Add(colEvent);
        }

        return events;
    }

    public Task<State> Reset()
    {
        var defPositions = _positionUtility.GetSpawnPositions(_mapSpec, _defenders.Count, false);
        var attPositions = _positionUtility.GetSpawnPositions(_mapSpec, _attackers.Count, true);

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
        bool terminated = _attackers.All(e => e.Destroyed);
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
