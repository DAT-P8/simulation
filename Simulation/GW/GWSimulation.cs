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

    private async Task<(State, GWSimulationInstance)> CreateNewSimulation(int pursuers, int evaders, MapSpec mapSpec)
    {
        List<GWDrone> pursuerDrones = new(pursuers);
        List<GWDrone> evaderDrones = new(evaders);

        for (int i = 0; i < pursuers; i++)
        {
            var d = await _droneSpawner.SpawnDroneAsync(i, false);
            pursuerDrones.Add(d);
        }

        for (int i = pursuers; i < evaders + pursuers; i++)
        {
            var a = await _droneSpawner.SpawnDroneAsync(i, true);
            evaderDrones.Add(a);
        }

        _mapSpawner.SpawnMap(mapSpec);

        var sim = new GWSimulationInstance(pursuerDrones, evaderDrones, _positionUtility, mapSpec, _logger, _id);
        var state = await sim.Reset();

        return (state, sim);
    }
}

public class GWSimulationInstance(
    List<GWDrone> pursuers,
    List<GWDrone> evaders,
    IPositionUtility positionUtility,
    MapSpec mapSpec,
    ILogger logger,
    long simId
) : IDisposable
{
    private readonly List<GWDrone> _pursuers = pursuers;
    private readonly List<GWDrone> _evaders = evaders;
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
        var drones = _pursuers.Concat(_evaders).ToList();
        List<(GWDrone drone, DroneAction action)> droneActions = drones.Join<GWDrone, DroneAction, long, (GWDrone, DroneAction)>(
            actions,
            drone => drone.Id,
            action => action.Id,
            (drone, action) => new(drone, action)
        ).ToList();

        if (droneActions.Any(e => e.drone.Destroyed))
            throw new Exception("Recieved action for a destroyed drone");

        droneActions = droneActions.Where(e => !e.drone.Destroyed).ToList();

        var collisions = DroneCollisionCheck(droneActions);
        var objectCollisions = ObjectCollisionIds(droneActions);
        var pursuersInTarget = PursuerTargetCheck(droneActions);
        var evadersInTarget = EvaderTargetCheck(droneActions);
        var outOfBounds = DronesOOB(droneActions);

        List<long> flatCollisions = [];
        foreach (var (drone1, drone2) in collisions)
            flatCollisions.AddRange([drone1, drone2]);

        objectCollisions = RemoveCollisions(objectCollisions, flatCollisions);
        pursuersInTarget = RemoveCollisions(pursuersInTarget, flatCollisions);
        evadersInTarget = RemoveCollisions(evadersInTarget, flatCollisions);
        outOfBounds = RemoveCollisions(outOfBounds, flatCollisions);

        var toDestroy = flatCollisions
            .Concat(outOfBounds)
            .Concat(objectCollisions);

        MoveDrones(actions);
        foreach (var id in toDestroy)
        {
            var drone = drones.FirstOrDefault(e => e.Id == id) ??
                throw new Exception($"Did not find a drone with id: {id}");

            drone.Destroyed = true;
        }

        List<Event> events = [];
        if (outOfBounds.Count != 0)
            events.Add(EventConstructor.MakeOutOfBoundsEvent(outOfBounds));
        if (evadersInTarget.Count != 0)
            events.Add(EventConstructor.MakeTargetReachedEvent(evadersInTarget));
        if (collisions.Count != 0)
            events.AddRange(collisions.Select(e => EventConstructor.MakeCollisionEvent(e)));
        if (pursuersInTarget.Count != 0)
            events.Add(EventConstructor.MakePursuerInTargetEvent(pursuersInTarget));
        if (objectCollisions.Count != 0)
            events.Add(EventConstructor.MakeObjectCollisionEvent(objectCollisions));

        return Task.FromResult(GetState(events));
    }

    private List<long> ObjectCollisionIds(List<(GWDrone drone, DroneAction actions)> drones)
    {
        var objects = _mapSpec.GetObjects();
        List<long> droneIds = CollisionChecker.ObjectCollisions(drones, objects);
        return droneIds;
    }

    private List<long> PursuerTargetCheck(List<(GWDrone drone, DroneAction actions)> drones)
    {
        var targetPositions = _positionUtility.GetTargetPositions(_mapSpec);
        var pursuers = drones.Where(e => !e.drone.IsEvader).ToList();
        List<long> droneIds = CollisionChecker.TargetCollisions(pursuers, targetPositions);
        return droneIds;
    }

    private List<long> EvaderTargetCheck(List<(GWDrone drone, DroneAction actions)> drones)
    {
        var targetPositions = _positionUtility.GetTargetPositions(_mapSpec);
        var evaders = drones.Where(e => e.drone.IsEvader).ToList();
        List<long> droneIds = CollisionChecker.TargetCollisions(evaders, targetPositions);
        return droneIds;
    }

    private List<long> DronesOOB(List<(GWDrone drone, DroneAction action)> drones)
    {
        List<(long id, Vector2I pos)> updatedPositions = drones.Select(e => (e.drone.Id, e.drone.GetPosition() + DtoMapper.ToVector(e.action))).ToList();
        var OOBDrones = updatedPositions.Where(e => !_positionUtility.IsInBounds(_mapSpec, e.pos));
        return [.. OOBDrones.Select(e => e.id)];
    }

    private static List<(long, long)> DroneCollisionCheck(List<(GWDrone drone, DroneAction actions)> drones)
    {
        return CollisionChecker.DroneCollisions(drones);
    }

    private static List<long> RemoveCollisions(List<long> eventDrones, List<long> collisionDrones)
    {
        return [.. eventDrones.Except(collisionDrones)];
    }

    private void MoveDrones(List<DroneAction> actions)
    {
        var drones = _pursuers.Concat(_evaders).ToList();
        foreach (var action in actions)
        {
            var drone = drones.FirstOrDefault(e => e.Id == action.Id) ??
                throw new Exception($"Did not find drone with id: {action.Id}");

            if (drone.Destroyed)
                throw new Exception($"Tried to move a destroyed drone with id: {action.Id}");

            var movementVec = DtoMapper.ToVector(action.Action) * (int)action.Velocity;
            var newPosition = drone.GetPosition() + movementVec;
            drone.SetPosition(newPosition);
        }
    }

    public Task<State> Reset()
    {
        var pursuerPositions = _positionUtility.GetPursuerSpawn(_mapSpec, _pursuers.Count);
        var evaderPositions = _positionUtility.GetEvaderSpawn(_mapSpec, _evaders.Count);

        foreach (var (drone, pos) in _pursuers.Zip(pursuerPositions))
            drone.SetPosition(new Vector2I(pos.X, pos.Y));
        foreach (var (drone, pos) in _evaders.Zip(evaderPositions))
            drone.SetPosition(new Vector2I(pos.X, pos.Y));

        foreach (var drone in _evaders.Concat(_pursuers))
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
        terminated = terminated || _evaders.All(e => e.Destroyed);

        State state = new()
        {
            Events = { events },
            Terminated = terminated,
            SimId = _simId,
            Objects = { _mapSpec.GetObjects() },
            DroneStates = {
                _evaders.Concat(_pursuers).Select(e => new DroneState {
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

        foreach (var d in _pursuers.Concat(_evaders))
            d.StaticBody2D.QueueFree();

        _pursuers.Clear();
        _evaders.Clear();
    }
}
