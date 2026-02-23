using System;
using System.Threading.Tasks;
using Godot;
using Grpc.Core;
using GWSimulation;
using Serilog;

namespace Simulation.GridEnvironment;

public class GWService(
    ILogger logger
) : GWSimulation.GWSimulation.GWSimulationBase
{
    private readonly ILogger _logger = logger;

    public override Task<GWCloseResponse> Close(GWCloseRequest request, ServerCallContext context)
    {
        _logger.Information("Close: {Request}", request);
        throw new NotImplementedException();
    }

    public override Task<GWActionResponse> DoStep(GWActionRequest request, ServerCallContext context)
    {
        _logger.Information("DoStep: {Request}", request);
        throw new NotImplementedException();
    }

    public override Task<GWResetResponse> Reset(GWResetRequest request, ServerCallContext context)
    {
        _logger.Information("Reset: {Request}", request);
        throw new NotImplementedException();
    }
}
