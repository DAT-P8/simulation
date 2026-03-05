using Serilog;
using TDFSimulation;

namespace Simulation.Lib.TDF;

public class TDFLoggingDecorator(TDFSimulation.TDFSimulation.TDFSimulationBase inner, ILogger logger) : TDFSimulation.TDFSimulation.TDFSimulationBase
{
    private readonly ILogger _logger = logger;
    private readonly TDFSimulation.TDFSimulation.TDFSimulationBase _inner = inner;

    public override Task<TDFCloseResponse> Close(TDFCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF Close: {Request}", request);
        return _inner.Close(request, context);
    }

    public override Task<TDFDoStepResponse> DoStep(TDFDoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF DoStep: {Request}", request);
        return _inner.DoStep(request, context);
    }

    public override Task<TDFNewResponse> New(TDFNewRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF New: {Request}", request);
        return _inner.New(request, context);
    }

    public override Task<TDFResetResponse> Reset(TDFResetRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF Reset: {Request}", request);
        return _inner.Reset(request, context);
    }
}
