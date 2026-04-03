using Grpc.Core;
using Serilog;

namespace Simulation.Lib;

public class Server(
    ILogger logger,
    string host,
    int port,
    GW2D.V1.SimulationService.SimulationServiceBase gwSimulationService,
    TDFSimulation.TDFSimulation.TDFSimulationBase tdfSimulationService
)
{
    private readonly ILogger _logger = logger;
    private readonly string _host = host;
    private readonly int _port = port;
    private readonly GW2D.V1.SimulationService.SimulationServiceBase _gwSimulationService = gwSimulationService;
    private readonly TDFSimulation.TDFSimulation.TDFSimulationBase _tdfSimulationService = tdfSimulationService;

    public void StartServer()
    {
        var server = new Grpc.Core.Server
        {
            Services = {
                GW2D.V1.SimulationService.BindService(_gwSimulationService),
                TDFSimulation.TDFSimulation.BindService(_tdfSimulationService)
            },
            Ports = { new ServerPort(_host, _port, ServerCredentials.Insecure) }
        };


        _logger.Information("Starting server on {Host}:{Port}", _host, _port);
        server.Start();
    }
}
