using Grpc.Core;
using Serilog;

namespace Simulation.Lib;

public class Server(
    ILogger logger,
    string host,
    int port,
    GWSimulation.GWSimulation.GWSimulationBase gwService
)
{
    private readonly ILogger _logger = logger;
    private readonly string _host = host;
    private readonly int _port = port;
    private readonly GWSimulation.GWSimulation.GWSimulationBase _gwService = gwService;

    public void StartServer()
    {
        var server = new Grpc.Core.Server
        {
            Services = {
                GWSimulation.GWSimulation.BindService(_gwService)
            },
            Ports = { new ServerPort(_host, _port, ServerCredentials.Insecure) }
        };

        _logger.Information("Starting server on {Host}:{Port}", _host, _port);
        server.Start();
    }
}
