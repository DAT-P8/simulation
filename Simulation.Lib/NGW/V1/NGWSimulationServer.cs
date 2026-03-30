using Grpc.Core;
using GW2D.V1;

namespace Simulation.Lib.NGW.V1;

public class NGWSimulationServer(
    INGWSimulationFactory simulationFactory
) : NGWSimulationService.NGWSimulationServiceBase, IDisposable
{
    private readonly INGWSimulationFactory _simulationFactory = simulationFactory;

    private readonly SemaphoreSlim _simSemaphore = new(1);
    private readonly Dictionary<long, INGWSimulation> _simulations = [];
    private readonly Dictionary<long, SemaphoreSlim> _simulationLocks = [];

    public override async Task<CloseResponse> Close(CloseRequest request, ServerCallContext context)
    {
        var id = request.SimId;

        if (!_simulations.TryGetValue(id, out var simulation))
            throw new Exception($"Did not find simulation with id: {id}!");
        if (!_simulationLocks.TryGetValue(id, out var simulationLock))
            throw new Exception($"Did not find the lock for simulation with id: {id}!");

        await simulationLock.WaitAsync();
        try
        {
            simulation.Close();
        }
        finally
        {
            simulationLock.Release();
        }

        return new CloseResponse { };
    }

    public override async Task<DoStepResponse> DoStep(DoStepRequest request, ServerCallContext context)
    {
        var id = request.SimId;
        var actions = request.DroneActions.ToList();

        if (!_simulations.TryGetValue(id, out var simulation))
            throw new Exception($"Did not find simulation with id: {id}!");
        if (!_simulationLocks.TryGetValue(id, out var simulationSemaphore))
            throw new Exception($"Did not find the lock for simulation with id: {id}!");

        NGWState state;
        await simulationSemaphore.WaitAsync();
        try
        {
            simulation.DoStep(actions);
            state = simulation.GetState();
        }
        finally
        {
            simulationSemaphore.Release();
        }

        return new DoStepResponse
        {
            StateResponse = new StateResponse
            {
                State = state,
            }
        };
    }

    public override async Task<NewResponse> New(NewRequest request, ServerCallContext context)
    {
        var evaders = request.EvaderCount;
        var pursuers = request.PursuerCount;
        var mapConfig = DtoMapper.MapSpecToMapConfiguration(request.Map);

        var simId = await CreateAndInsertSimulation(evaders, pursuers, mapConfig);

        if (!_simulations.TryGetValue(simId, out var simulation))
            throw new Exception("Somehow the just created simulation could not be found!");

        return new NewResponse
        {
            StateResponse = new StateResponse
            {
                State = simulation.GetState(),
            }
        };
    }

    public override async Task<ResetResponse> Reset(ResetRequest request, ServerCallContext context)
    {
        var id = request.SimId;

        if (!_simulations.TryGetValue(id, out var simulation))
            throw new Exception($"Did not find simulation with id: {id}!");
        if (!_simulationLocks.TryGetValue(id, out var simulationSemaphore))
            throw new Exception($"Did not find the lock for simulation with id: {id}!");

        NGWState state;
        await simulationSemaphore.WaitAsync();
        try
        {
            simulation.Reset();
            state = simulation.GetState();
        }
        finally
        {
            simulationSemaphore.Release();
        }

        return new ResetResponse
        {
            StateResponse = new StateResponse
            {
                State = state,
            }
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // Disposable stuff
    }

    private async Task<long> CreateAndInsertSimulation(long evaders, long pursuers, MapConfiguration mapConfig)
    {
        await _simSemaphore.WaitAsync();

        try
        {
            long newId = _simulations.Keys.Max() + 1;

            var simulation = _simulationFactory.CreateSimulation(newId, evaders, pursuers, mapConfig);
            _simulations.Add(newId, simulation);
            _simulationLocks.Add(newId, new SemaphoreSlim(1));
            return newId;
        }
        finally
        {
            _simSemaphore.Release();
        }
    }
}
