using Grpc.Core;
using GWSimulation;
using Serilog;

namespace Simulation.Lib;

public class GWSimulationMultiplexer : GWSimulation.GWSimulation.GWSimulationBase, IDisposable
{
    private readonly Dictionary<long, (IGWSimulation, DateTime)> _simulations = [];
    private readonly ILogger _logger;
    private readonly IGWSimulationFactory _simulationFactory;
    private readonly SemaphoreSlim _simulationSemaphore = new(1);
    private readonly Timer _timer;

    public GWSimulationMultiplexer(IGWSimulationFactory simulationFactory, ILogger logger)
    {
        _logger = logger;
        _simulationFactory = simulationFactory;
        _timer = new Timer(CheckCleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private void CheckCleanup(object? state)
    {
        var threshold = DateTime.UtcNow - TimeSpan.FromMinutes(1);

        _simulationSemaphore.Wait();
        try {
            var toClean = _simulations.Where(e => e.Value.Item2 < threshold).ToList();
            foreach (var (key, (sim, _)) in toClean)
            {
                _logger.Warning("Cleaned up simulation {Id} due to being idle for too long!", key);
                sim.Close();
                _simulations.Remove(key);
            }
        }
        finally {
            _simulationSemaphore.Release();
        }
    }

    public override async Task<GWActionResponse> DoStep(GWActionRequest request, ServerCallContext context)
    {
        if (!_simulations.TryGetValue(request.Id, out (IGWSimulation, DateTime) simDate))
        {
            return new GWActionResponse
            {
                ErrorMessage = $"The simulation with id={request.Id} doesn't exist!"
            };
        }

        _simulations[request.Id] = (simDate.Item1, DateTime.UtcNow);

        var (sim, _) = simDate;
        var newState = await sim.DoStep([.. request.DroneActions]);

        return new GWActionResponse
        {
            State = newState
        };
    }

    public override async Task<GWResetResponse> Reset(GWResetRequest request, ServerCallContext context)
    {
        if (!_simulations.TryGetValue(request.Id, out (IGWSimulation, DateTime) simDate))
        {
            throw new Exception($"Cannot reset non-existing simulation {request.Id}");
        }

        _simulations[request.Id] = (simDate.Item1, DateTime.UtcNow);

        var (sim, _) = simDate;
        var state = await sim.Reset();

        return new GWResetResponse
        {
            State = state
        };
    }

    public override async Task<GWNewResponse> New(GWNewRequest request, ServerCallContext context)
    {
        var newSim = await _simulationFactory.CreateSimulation();

        long id;
        await _simulationSemaphore.WaitAsync();
        try
        {
            id = GetNewId();
            _simulations.Add(id, (newSim, DateTime.UtcNow));
        }
        finally
        {
            _simulationSemaphore.Release();
        }

        var state = await newSim.Reset();
        return new GWNewResponse
        {
            Id = id,
            State = state
        };
    }

    public override async Task<GWCloseResponse> Close(GWCloseRequest request, ServerCallContext context)
    {
        if (_simulations.TryGetValue(request.Id, out var simDate))
        {
            var (sim, _) = simDate;

            await sim.Close();
            await _simulationSemaphore.WaitAsync();

            try
            {
                _simulations.Remove(request.Id);
            }
            finally
            {
                _simulationSemaphore.Release();
            }
        }

        return new GWCloseResponse { };
    }

    private long GetNewId()
    {
        long newId;
        if (_simulations.Count > 0)
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

public interface IGWSimulationFactory
{
    Task<IGWSimulation> CreateSimulation();
}

public interface IGWSimulation
{
    Task<GWState> Reset();
    Task Close();
    Task<GWState> DoStep(List<GWDroneAction> actions);
}
