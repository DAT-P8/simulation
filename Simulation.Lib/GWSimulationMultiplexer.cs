using Grpc.Core;
using GWSimulation;
using Serilog;

namespace Simulation.Lib;

public class GWSimulationMultiplexer(IGWSimulationFactory simulationFactory, ILogger logger) : GWSimulation.GWSimulation.GWSimulationBase
{
    private readonly ILogger _logger = logger;

    private readonly Dictionary<long, IGWSimulation> _simulations = [];
    private readonly IGWSimulationFactory _simulationFactory = simulationFactory;
    private readonly SemaphoreSlim _simSemaphore = new(1);

    public override async Task<GWActionResponse> DoStep(GWActionRequest request, ServerCallContext context)
    {
        _logger.Information("DoStep: {Request}", request.Id, request);
        if (!_simulations.TryGetValue(request.Id, out var sim))
        {
            return new GWActionResponse
            {
                ErrorMessage = $"The simulation with id={request.Id} doesn't exist!"
            };
        }

        var newState = await sim.DoStep([.. request.DroneActions]);
        return new GWActionResponse
        {
            State = newState
        };
    }

    public override async Task<GWResetResponse> Reset(GWResetRequest request, ServerCallContext context)
    {
        _logger.Information("Reset: {Request}", request);
        GWState state;
        if (_simulations.TryGetValue(request.Id, out var simulation))
        {
            state = await simulation.Reset();
        }
        else
        {
            _logger.Error("Attempt to reset a non-existing simulation {Id}", request.Id);
            throw new Exception($"Cannot reset non-existing simulation {request.Id}");
        }

        return new GWResetResponse
        {
            State = state
        };
    }

    public override async Task<GWNewResponse> New(GWNewRequest request, ServerCallContext context)
    {
        _logger.Information("New: {Request}", request);
        var newSim = await _simulationFactory.CreateSimulation();

        long id;
        await _simSemaphore.WaitAsync();
        try
        {
            id = GetNewId();
            _simulations.Add(id, newSim);
        }
        finally
        {
            _simSemaphore.Release();
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
        _logger.Information("Close: {Request}", request);
        if (_simulations.TryGetValue(request.Id, out var sim))
        {
            await sim.Close();

            await _simSemaphore.WaitAsync();

            try
            {
                _simulations.Remove(request.Id);
            }
            finally
            {
                _simSemaphore.Release();
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
