using Grpc.Core;
using GW2D.V1;
using Serilog;

namespace Simulation.Lib.GW;

public class GWSimulationServer : SimulationService.SimulationServiceBase, IDisposable
{
    private readonly Dictionary<long, SimulationDatetime> _simulations = [];

    private readonly ILogger _logger;
    private readonly IGWSimulationFactory _simulationFactory;
    private readonly SemaphoreSlim _simulationSemaphore = new(1);
    private readonly Timer _timer;

    public GWSimulationServer(IGWSimulationFactory simulationFactory, ILogger logger)
    {
        _logger = logger;
        _simulationFactory = simulationFactory;
        _timer = new Timer(CheckCleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private void CheckCleanup(object? state)
    {
        var threshold = DateTime.UtcNow - TimeSpan.FromMinutes(1);

        _simulationSemaphore.Wait();
        try
        {
            var toClean = _simulations.Where(e => e.Value.DateTime < threshold).ToList();
            foreach (var (key, simDate) in toClean)
            {
                _logger.Warning("Cleaned up simulation {Id} due to being idle for too long!", key);
                simDate.Simulation.Close();
                _simulations.Remove(key);
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

        // IGWSimulation newSim = await _simulationFactory.CreateSimulation();
        long id;
        await _simulationSemaphore.WaitAsync();
        try
        {
            id = GetNewId();
            newSim = await _simulationFactory.CreateSimulation(id);
            _simulations.Add(id, new SimulationDatetime(newSim, DateTime.UtcNow));
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
                _simulations.Remove(request.SimId);
            }
            finally
            {
                _simulationSemaphore.Release();
            }
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
