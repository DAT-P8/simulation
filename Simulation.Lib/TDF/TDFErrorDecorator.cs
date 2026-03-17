using Serilog;
using TDFSimulation;

namespace Simulation.Lib.TDF;

public class TDFErrorDecorator(TDFSimulation.TDFSimulation.TDFSimulationBase inner, ILogger logger) : TDFSimulation.TDFSimulation.TDFSimulationBase
{
    private readonly ILogger _logger = logger;
    private readonly TDFSimulation.TDFSimulation.TDFSimulationBase _inner = inner;

    public override async Task<TDFCloseResponse> Close(TDFCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.Close(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling Close with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override async Task<TDFDoStepResponse> DoStep(TDFDoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.DoStep(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling DoStep with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override async Task<TDFNewResponse> New(TDFNewRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.New(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling New with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override async Task<TDFResetResponse> Reset(TDFResetRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return await _inner.Reset(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling Reset with {Request}: {Error}", request, e);
            throw;
        }
    }
}
