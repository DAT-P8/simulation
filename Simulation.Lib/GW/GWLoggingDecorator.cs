using GW2D.V1;
using Serilog;

namespace Simulation.Lib.GW;

public class GWLoggingDecorator(
    SimulationService.SimulationServiceBase inner,
    ILogger logger
) : SimulationService.SimulationServiceBase
{
    private readonly ILogger _logger = logger;
    private readonly SimulationService.SimulationServiceBase _inner = inner;

    public override Task<CloseResponse> Close(CloseRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW Close: {Request}", request);
        return _inner.Close(request, context);
    }

    public override Task<DoStepResponse> DoStep(DoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW DoStep: {Request}", request);
        return _inner.DoStep(request, context);
    }

    public override Task<NewResponse> New(NewRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW New: {Request}", request);
        return _inner.New(request, context);
    }

    public override Task<ResetResponse> Reset(ResetRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW Reset: {Reset}", request);
        return _inner.Reset(request, context);
    }
}
