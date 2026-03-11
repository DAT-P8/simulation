using Serilog;
using TDFSimulation;

namespace Simulation.Lib.TDF;

public class TDFErrorDecorator(TDFSimulation.TDFSimulation.TDFSimulationBase inner, ILogger logger) : TDFSimulation.TDFSimulation.TDFSimulationBase
{
    private readonly ILogger _logger = logger;
    private readonly TDFSimulation.TDFSimulation.TDFSimulationBase _inner = inner;

    public override Task<TDFCloseResponse> Close(TDFCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.Close(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling Close with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override Task<TDFDoStepResponse> DoStep(TDFDoStepRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.DoStep(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling DoStep with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override Task<TDFNewResponse> New(TDFNewRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.New(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling New with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override Task<TDFResetResponse> Reset(TDFResetRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.Reset(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("TDF: Error calling Reset with {Request}: {Error}", request, e);
            throw;
        }
    }
}
