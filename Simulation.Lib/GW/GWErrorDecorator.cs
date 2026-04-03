using GW2D.V1;
using Serilog;

namespace Simulation.Lib.GW;

public class GWErrorDecorator(
    SimulationService.SimulationServiceBase inner,
    ILogger logger
) : SimulationService.SimulationServiceBase
{
    private readonly ILogger _logger = logger;
    private readonly SimulationService.SimulationServiceBase _inner = inner;

    public override async Task<CloseResponse> Close(CloseRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.Close(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("GW: Error calling Close with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override async Task<DoStepResponse> DoStep(DoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.DoStep(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("GW: Error calling DoStep with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override async Task<NewResponse> New(NewRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.New(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("GW: Error calling New with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override async Task<ResetResponse> Reset(ResetRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.Reset(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("GW: Error calling Reset with {Request}: {Error}", request, e);
            throw;
        }
    }
}
