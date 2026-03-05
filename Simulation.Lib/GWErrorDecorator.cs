using GWSimulation;
using Serilog;

namespace Simulation.Lib;

public class GWErrorDecorator(
    GWSimulation.GWSimulation.GWSimulationBase inner,
    ILogger logger
) : GWSimulation.GWSimulation.GWSimulationBase
{
    private readonly ILogger _logger = logger;
    private readonly GWSimulation.GWSimulation.GWSimulationBase _inner = inner;

    public override Task<GWCloseResponse> Close(GWCloseRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.Close(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("Error calling Close with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override Task<GWActionResponse> DoStep(GWActionRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.DoStep(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("Error calling DoStep with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override Task<GWNewResponse> New(GWNewRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.New(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("Error calling New with {Request}: {Error}", request, e);
            throw;
        }
    }

    public override Task<GWResetResponse> Reset(GWResetRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            return _inner.Reset(request, context);
        }
        catch (Exception e)
        {
            _logger.Error("Error calling Reset with {Request}: {Error}", request, e);
            throw;
        }
    }
}
