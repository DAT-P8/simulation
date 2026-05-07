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

    public async override Task<CloseResponse> Close(CloseRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Debug("GW Close Request: {Id}", request.SimId);
        var r = await _inner.Close(request, context);
        _logger.Debug("GW Close Response: {Id}", request.SimId);

        return r;
    }

    public async override Task<DoStepResponse> DoStep(DoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Debug("GW DoStep Request: {Id}", request.SimId);
        var r = await _inner.DoStep(request, context);
        _logger.Debug("GW DoStep Response: {Id}", request.SimId);

        return r;
    }

    public async override Task<NewResponse> New(NewRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Debug("GW New Request");
        var r = await _inner.New(request, context);
        _logger.Debug("GW New Response");

        return r;
    }

    public async override Task<ResetResponse> Reset(ResetRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.Debug("GW Reset Request: {Id}", request.SimId);
        var r = await _inner.Reset(request, context);
        _logger.Debug("GW Reset Response: {Id}", request.SimId);

        return r;
    }
}
