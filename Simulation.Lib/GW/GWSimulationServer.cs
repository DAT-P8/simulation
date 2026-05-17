using System.Collections.Concurrent;
using Grpc.Core;
using GW2D.V1;
using Serilog;

namespace Simulation.Lib.GW;

public class GWSimulationServer : SimulationService.SimulationServiceBase, IDisposable
{
    // Idle threshold: any sim that hasn't received a DoStep/Reset/New in this
    // many minutes is considered abandoned and reaped. Must be longer than
    // the slowest-imaginable algo init on the client side — RLlib algo setup
    // can take 2+ minutes when spawning many env runners, during which a
    // freshly-created sim sits idle waiting to be used.
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromMinutes(10);

    // How often the cleanup sweep runs. Doesn't need to be tight — the cost
    // of a slightly-stale sim hanging around is just a bit of memory.
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(2);

    private readonly ConcurrentDictionary<long, SimulationDatetime> _simulations = [];

    private readonly ILogger _logger;
    private readonly IGWSimulationFactory _simulationFactory;
    private readonly SemaphoreSlim _simulationSemaphore = new(1);
    private readonly Timer _timer;

    public GWSimulationServer(IGWSimulationFactory simulationFactory, ILogger logger)
    {
        _logger = logger;
        _simulationFactory = simulationFactory;
        _timer = new Timer(CheckCleanup, null, TimeSpan.Zero, CleanupInterval);
    }

    private void CheckCleanup(object? state)
    {
        var threshold = DateTime.UtcNow - IdleThreshold;

        _simulationSemaphore.Wait();
        try
        {
            var toClean = _simulations.Where(e => e.Value.DateTime < threshold).ToList();
            foreach (var (key, simDate) in toClean)
            {
                _logger.Warning("Cleaned up simulation {Id} due to being idle for too long!", key);
                simDate.Simulation.Close();
                _simulations.Remove(key, out _);
            }
        }
        finally
        {
            _simulationSemaphore.Release();
        }
    }

    public override async Task<DoStepResponse> DoStep(DoStepRequest request, ServerCallContext context)
    {
        if (!_simulations.TryGetValue(request.SimId, out var simDate))
        {
            return new DoStepResponse
            {
                StateResponse = new StateResponse
                {
                    ErrorMessage = $"The simulation with id={request.SimId} doesn't exist!"
                }
            };
        }
        simDate.DateTime = DateTime.UtcNow;

        var newState = await simDate.Simulation.DoStep([.. request.DroneActions]);

        return new DoStepResponse
        {
            StateResponse = new StateResponse
            {
                State = newState
            }
        };
    }

    public override async Task<ResetResponse> Reset(ResetRequest request, ServerCallContext context)
    {
        if (!_simulations.TryGetValue(request.SimId, out var simDate))
        {
            return new ResetResponse
            {
                StateResponse = new StateResponse
                {
                    ErrorMessage = $"The simulation with id={request.SimId} doesn't exist!"
                }
            };
        }

        simDate.DateTime = DateTime.UtcNow;
        var state = await simDate.Simulation.Reset();


        return new ResetResponse
        {
            StateResponse = new StateResponse
            {
                State = state
            }
        };
    }

    public override async Task<NewResponse> New(NewRequest request, ServerCallContext context)
    {
        IGWSimulation newSim;

        long id;
        await _simulationSemaphore.WaitAsync();
        try
        {
            id = GetNewId();
            newSim = await _simulationFactory.CreateSimulation(id);
            // Stamp the creation time so the cleanup sweep doesn't immediately
            // age this sim out before the client gets a chance to use it.
            // (The SimulationDatetime constructor already does this, but being
            // explicit here makes the intent clear.)
            _simulations.TryAdd(id, new SimulationDatetime(newSim, DateTime.UtcNow));
        }
        finally
        {
            _simulationSemaphore.Release();
        }

        var state = await newSim.New(
            request.Map,
            (int)request.EvaderCount,
            (int)request.PursuerCount
        );

        _logger.Information("Created sim {Id}; active sim count: {Count}", id, _simulations.Count);

        return new NewResponse
        {
            StateResponse = new StateResponse
            {
                State = state
            }
        };
    }

    public override async Task<CloseResponse> Close(CloseRequest request, ServerCallContext context)
    {
        if (_simulations.TryGetValue(request.SimId, out var simDate))
        {
            await simDate.Simulation.Close();
            await _simulationSemaphore.WaitAsync();

            try
            {
                _simulations.TryRemove(request.SimId, out _);
            }
            finally
            {
                _simulationSemaphore.Release();
            }
            _logger.Information("Closed sim {Id}; active sim count: {Count}", request.SimId, _simulations.Count);
        }
        else
        {
            _logger.Warning("Close called on unknown sim {Id}", request.SimId);
        }

        return new CloseResponse { };
    }

    private record SimulationDatetime(IGWSimulation Simulation, DateTime DateTime)
    {
        public DateTime DateTime = DateTime;
    }

    private long GetNewId()
    {
        long newId;
        if (!_simulations.IsEmpty)
            newId = _simulations.Select((d, _) => d.Key).Max() + 1;
        else
            newId = 1;

        return newId;
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}
