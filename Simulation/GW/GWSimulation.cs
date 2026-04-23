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

    public async Task<State> New(MapSpec mapSpec, int evaders, int pursuers)
    {
        if (_instance is not null)
            throw new Exception("Attempted override an existing simulation!");

        var (state, newInstance) = await CreateNewSimulation(pursuers, evaders, mapSpec);
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

    private async Task<(State, GWSimulationInstance)> CreateNewSimulation(int defenders, int attackers, MapSpec mapSpec)
    {
        List<GWDrone> defenderDrones = new(defenders);
        List<GWDrone> attackerDrones = new(attackers);

        for (int i = 0; i < defenders; i++)
        {
            var d = _droneSpawner.SpawnDrone(i, false);
            defenderDrones.Add(d);
        }

        for (int i = defenders; i < attackers + defenders; i++)
        {
            var a = _droneSpawner.SpawnDrone(i, true);
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
    // This distance can be very low as all collisions with static objects/static targets
    // will exactly pass through their point (origin in relative space).
    private const float AREA_COLLISION_THRESHOLD = 1e-2f;

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
        var drones = _defenders.Concat(_attackers).Where(d => !d.Destroyed).ToList();

        var before = drones.Select(e => e.GetPosition()).ToList();
        MoveDrones(actions);
        var after = drones.Select(e => e.GetPosition()).ToList();
        var zipped = drones
            .Zip(before.Zip(after))
            .Select(e =>
            {
                var (drone, (before, after)) = e;
                return (drone, before, after);
            })
            .ToList();

        var collisions = CollisionCheck(zipped);
        var collisionsWithObjects = CollisionWithObjectsCheck(zipped);
        var outOfBounds = BoundsCheck(zipped);
        var targetReached = TargetCheck(zipped);
        var defenderEnteredTarget = PursuerEnteredTargetCheck(zipped);

        collisionsWithObjects = RemoveCollidedFromObjectCollisions(collisionsWithObjects, collisions);
        outOfBounds = RemoveCollidedFromOutOfBounds(collisions, outOfBounds);
        defenderEnteredTarget = RemoveCollidedFromPursuerEnteredTarget(collisions, defenderEnteredTarget);
        targetReached = RemoveCollidedFromTargetReached(collisions, targetReached);

        var toDestroy = collisions
            .SelectMany(e => e.DroneIds)
            .Concat(outOfBounds.DroneIds)
            .Concat(collisionsWithObjects.DroneIds);

        foreach (var id in toDestroy)
        {
            var drone = drones.FirstOrDefault(e => e.Id == id) ??
                throw new Exception($"Did not find a drone with id: {id}");

            drone.Destroyed = true;
        }

        List<Event> events = [];
        if (outOfBounds.DroneIds.Count != 0)
            events.Add(new Event { OutOfBoundsEvent = outOfBounds });
        if (targetReached.DroneIds.Count != 0)
            events.Add(new Event { TargetReachedEvent = targetReached });
        if (collisions.Any(e => e.DroneIds.Count != 0))
            events.AddRange(collisions.Select(e => new Event { CollisionEvent = e }));

        return Task.FromResult(GetState(events));
    }

    private DroneObjectCollisionEvent RemoveCollidedFromObjectCollisions(DroneObjectCollisionEvent collisionsWithObjects, List<CollisionEvent> collisions)
    {
        var collisionSet = collisions.SelectMany(e => e.DroneIds).ToHashSet();
        var collisionObjectSet = collisionsWithObjects.DroneIds.ToHashSet();

        var newSet = collisionObjectSet.ToHashSet();
        foreach (var id in collisionSet.Intersect(collisionObjectSet))
        {
            newSet.Remove(id);
        }

        return new DroneObjectCollisionEvent { DroneIds = { newSet } };
    }

    private DroneObjectCollisionEvent CollisionWithObjectsCheck(List<(GWDrone drone, Vector2I before, Vector2I after)> zipped)
    {
        var objects = _mapSpec.GetObjects();
        var objectPositions = objects
            .Select(e => e.GetPosition())
            .Select(e => new Vector3D<float>(e.X, e.Y, e.Z))
            .ToList();

        List<long> ids = [];
        foreach (var (drone, before, after) in zipped)
        {
            var beforeVec = new Vector3D<float>(before.X, 0, before.Y);
            var afterVec = new Vector3D<float>(after.X, 0, after.Y);
            foreach (var objectPos in objectPositions)
            {
                var point = VectorExtensions.SweepPair(beforeVec, afterVec, objectPos, objectPos);
                if (point.Dot(point) <= AREA_COLLISION_THRESHOLD)
                {
                    ids.Add(drone.Id);
                    break;
                }
            }
        }

        return new DroneObjectCollisionEvent { DroneIds = { ids } };
    }

    private PursuerEnteredTargetEvent RemoveCollidedFromPursuerEnteredTarget(List<CollisionEvent> collisions, PursuerEnteredTargetEvent defenderEnteredTarget)
    {
        var collisionIds = collisions.SelectMany(e => e.DroneIds).ToHashSet();
        var defenderIds = defenderEnteredTarget.DroneIds.ToHashSet();

        foreach (var id in defenderIds.Intersect(collisionIds))
            defenderIds.Remove(id);

        return new PursuerEnteredTargetEvent
        {
            DroneIds = { defenderIds }
        };
    }

    private PursuerEnteredTargetEvent PursuerEnteredTargetCheck(List<(GWDrone drone, Vector2I before, Vector2I after)> zipped)
    {
        var targetPositions = _positionUtility.GetTargetPositions(_mapSpec).Select(p => new Vector2I(p.X, p.Z));

        List<long> inTarget = [];
        foreach (var (drone, before, after) in zipped)
        {
            if (drone.IsEvader) continue;
            var b = new Vector3D<float>(before.X, 0, before.Y);
            var a = new Vector3D<float>(after.X, 0, after.Y);

            foreach (var tp in targetPositions)
            {
                var target = new Vector3D<float>(tp.X, 0, tp.Y);
                var point = VectorExtensions.SweepPair(b, a, target, target);

                if (point.Dot(point) <= AREA_COLLISION_THRESHOLD)
                {
                    inTarget.Add(drone.Id);
                    break;
                }

            }
        }

        return new PursuerEnteredTargetEvent
        {
            DroneIds = { inTarget }
        };
    }

    private TargetReachedEvent RemoveCollidedFromTargetReached(List<CollisionEvent> collisions, TargetReachedEvent targetReached)
    {
        var idSet = targetReached.DroneIds.ToHashSet();
        var collisionIds = collisions.SelectMany(e => e.DroneIds).ToList();

        foreach (var id in collisionIds.Intersect(idSet))
            idSet.Remove(id);

        return new TargetReachedEvent
        {
            DroneIds = { idSet }
        };
    }

    private OutOfBoundsEvent RemoveCollidedFromOutOfBounds(List<CollisionEvent> collisions, OutOfBoundsEvent outOfBounds)
    {
        var idSet = outOfBounds.DroneIds.ToHashSet();
        var collisionIds = collisions.SelectMany(e => e.DroneIds).ToList();

        foreach (var id in collisionIds.Intersect(idSet))
            idSet.Remove(id);

        return new OutOfBoundsEvent
        {
            DroneIds = { idSet }
        };
    }

    private TargetReachedEvent TargetCheck(List<(GWDrone drone, Vector2I before, Vector2I after)> zipped)
    {
        List<long> onTarget = [];
        foreach (var (drone, _, after) in zipped)
        {
            if (!drone.IsEvader) continue;

            if (!_positionUtility.IsOnTarget(_mapSpec, new Vector3I(after.X, 0, after.Y)))
                continue;

            onTarget.Add(drone.Id);
        }

        return new TargetReachedEvent
        {
            DroneIds = { onTarget }
        };
    }

    private OutOfBoundsEvent BoundsCheck(List<(GWDrone drone, Vector2I before, Vector2I after)> zipped)
    {
        List<long> ids = [];
        foreach (var (drone, _, after) in zipped)
        {
            if (!_positionUtility.IsInBounds(_mapSpec, new Vector3I(after.X, 0, after.Y)))
                ids.Add(drone.Id);
        }

        var outOfbounds = new OutOfBoundsEvent()
        {
            DroneIds = { ids }
        };

        return outOfbounds;
    }

    private List<CollisionEvent> CollisionCheck(List<(GWDrone drone, Vector2I before, Vector2I after)> zipped)
    {
        List<(long, long)> pairs = [];
        for (int i = 0; i < zipped.Count; i++)
        {
            for (int j = i + 1; j < zipped.Count; j++)
            {
                var (d1, b1, a1) = zipped[i];
                var (d2, b2, a2) = zipped[j];

                var point = VectorExtensions.SweepPair(
                    new Vector3D<float>(b1.X, 0, b1.Y),
                    new Vector3D<float>(a1.X, 0, a1.Y),
                    new Vector3D<float>(b2.X, 0, b2.Y),
                    new Vector3D<float>(a2.X, 0, a2.Y)
                );

                if (point.Dot(point) <= .5)
                {
                    pairs.Add((d1.Id, d2.Id));
                }
            }
        }

        return [.. pairs.Select(e => {
            return new CollisionEvent
            {
                DroneIds = { e.Item1, e.Item2 }
            };
        })];
    }

    private void MoveDrones(List<DroneAction> actions)
    {
        var drones = _defenders.Concat(_attackers).ToList();
        foreach (var action in actions)
        {
            var drone = drones.FirstOrDefault(e => e.Id == action.Id) ??
                throw new Exception($"Did not find drone with id: {action.Id}");

            if (drone.Destroyed) continue;

            var movementVec = DtoMapper.ToVector(action.Action) * (int)action.Velocity;
            var newPosition = drone.GetPosition() + movementVec;
            drone.SetPosition(newPosition);
        }
    }


    public Task<State> Reset()
    {
        var defPositions = _positionUtility.GetSpawnPositions(_mapSpec, _defenders.Count, false);
        var attPositions = _positionUtility.GetSpawnPositions(_mapSpec, _attackers.Count, true);

        foreach (var (drone, pos) in _defenders.Zip(defPositions))
            drone.SetPosition(new Vector2I(pos.X, pos.Z));
        foreach (var (drone, pos) in _attackers.Zip(attPositions))
            drone.SetPosition(new Vector2I(pos.X, pos.Z));

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
        bool terminated = false;
        foreach (var ev in events)
        {
            if (
                ev.EventOneofCase == Event.EventOneofOneofCase.TargetReachedEvent &&
                ev.TargetReachedEvent.DroneIds.Count != 0
            )
            {
                terminated = true;
                break;
            }
        }
        terminated = terminated || _attackers.All(e => e.Destroyed);

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
}
