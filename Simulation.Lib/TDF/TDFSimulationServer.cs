using Serilog;
using TDFSimulation;

namespace Simulation.Lib.TDF;

public class TDFSimulationServer(ITDFSimulationFactory simulationFactory, ILogger logger) : TDFSimulation.TDFSimulation.TDFSimulationBase, IDisposable
{
    private readonly ILogger _logger = logger;
    private readonly Dictionary<long, SimulationDate> _simulations = [];
    private readonly SemaphoreSlim _simulationsSemaphore = new(1);
    private readonly ITDFSimulationFactory _simulationFactory = simulationFactory;

    public override async Task<TDFCloseResponse> Close(TDFCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        await _simulationsSemaphore.WaitAsync();
        SimulationDate? simulationDate;
        try
        {
            if (!_simulations.TryGetValue(request.Id, out simulationDate))
            {
                return new TDFCloseResponse
                {
                    ErrorMsg = $"Simulation with id {request.Id} doesn't exist."
                };
            }

            _simulations.Remove(request.Id);
        }
        finally
        {
            _simulationsSemaphore.Release();
        }

        if (simulationDate != null)
            await simulationDate.Simulation.Close();
        else
            _logger.Error("Close: Somehow simulation was null: {Request}", request);

        return new TDFCloseResponse();
    }

    public override async Task<TDFDoStepResponse> DoStep(TDFDoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        if (!_simulations.TryGetValue(request.Id, out SimulationDate? simulationDate))
        {
            return new TDFDoStepResponse
            {
                ErrorMsg = $"Simulation with id {request.Id} doesn't exist."
            };
        }

        if (simulationDate == null)
        {
            _logger.Error("DoStep: Somehow simulation was null: {Request}", request);
            return new TDFDoStepResponse
            {
                ErrorMsg = $"DoStep: Somehow simulation was null for id: {request.Id}"
            };
        }

        var state = await simulationDate.Simulation.DoStep([.. request.DroneActions]);

        return new TDFDoStepResponse
        {
            State = state
        };
    }

    public override async Task<TDFNewResponse> New(TDFNewRequest request, Grpc.Core.ServerCallContext context)
    {
        ITDFSimulation simulation;
        await _simulationsSemaphore.WaitAsync();
        try
        {
            var id = GetNewId();
            simulation = await _simulationFactory.CreateSimulation(id, request.EvaderCount, request.PursuerCount, request.EvaderDomeRadius, request.PursuerDomeRadius, request.ArenaDomeRadius);
            _simulations.Add(id, new SimulationDate(simulation, DateTime.UtcNow));
        }
        finally
        {
            _simulationsSemaphore.Release();
        }

        var state = await simulation.New();

        return new TDFNewResponse
        {
            State = state
        };
    }

    public override async Task<TDFResetResponse> Reset(TDFResetRequest request, Grpc.Core.ServerCallContext context)
    {
        if (!_simulations.TryGetValue(request.Id, out SimulationDate? simulationDate))
        {
            return new TDFResetResponse
            {
                ErrorMsg = $"Simulation with id {request.Id} doesn't exist."
            };
        }

        if (simulationDate == null)
        {
            _logger.Error("DoStep: Somehow simulation was null: {Request}", request);
            return new TDFResetResponse
            {
                ErrorMsg = $"DoStep: Somehow simulation was null for id: {request.Id}"
            };
        }

        var state = await simulationDate.Simulation.Reset();

        return new TDFResetResponse
        {
            State = state
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private record SimulationDate(ITDFSimulation Simulation, DateTime LastCalled)
    {
        public readonly ITDFSimulation Simulation = Simulation;
        public DateTime LastCalled = LastCalled;
    }

    private long GetNewId()
    {
        if (_simulations.Count == 0)
            return 1;
        return _simulations.Max(e => e.Key) + 1;
    }
}
