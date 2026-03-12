using Serilog;
using TDFSimulation;

namespace Simulation.Lib.TDF;

public class TDFLoggingDecorator(TDFSimulation.TDFSimulation.TDFSimulationBase inner, ILogger logger) : TDFSimulation.TDFSimulation.TDFSimulationBase
{
    private readonly ILogger _logger = logger;
    private readonly TDFSimulation.TDFSimulation.TDFSimulationBase _inner = inner;

    public override async Task<TDFCloseResponse> Close(TDFCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF Close: {Request}", request);
        return await _inner.Close(request, context);
    }

    public override async Task<TDFDoStepResponse> DoStep(TDFDoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF DoStep: {Request}", request);
        return await _inner.DoStep(request, context);
    }

    public override async Task<TDFNewResponse> New(TDFNewRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF New: {Request}", request);
        return await _inner.New(request, context);
    }

    public override async Task<TDFResetResponse> Reset(TDFResetRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Information("TDF Reset: {Request}", request);
        return await _inner.Reset(request, context);
    }
}
