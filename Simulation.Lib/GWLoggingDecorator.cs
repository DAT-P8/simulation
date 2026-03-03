using GWSimulation;
using Serilog;

namespace Simulation.Lib;

public class GWLoggingDecorator(
    GWSimulation.GWSimulation.GWSimulationBase inner,
    ILogger logger
) : GWSimulation.GWSimulation.GWSimulationBase
{
    private readonly ILogger _logger = logger;
    private readonly GWSimulation.GWSimulation.GWSimulationBase _inner = inner;

    public override Task<GWCloseResponse> Close(GWCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW Close: {Request}", request);
        return _inner.Close(request, context);
    }

    public override Task<GWActionResponse> DoStep(GWActionRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW DoStep: {Request}", request);
        return _inner.DoStep(request, context);
    }

    public override Task<GWNewResponse> New(GWNewRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW New: {Request}", request);
        return _inner.New(request, context);
    }

    public override Task<GWResetResponse> Reset(GWResetRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("GW Reset: {Reset}", request);
        return _inner.Reset(request, context);
    }
}
