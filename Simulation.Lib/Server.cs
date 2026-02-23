using Grpc.Core;
using Serilog;

namespace Simulation.Lib;

public class Server(
    ILogger logger,
    string host,
    int port,
    IGWSimulationFactory gwSimulationFactory
)
{
    private readonly ILogger _logger = logger;
    private readonly string _host = host;
    private readonly int _port = port;
    private readonly IGWSimulationFactory _gwSimulationFactory = gwSimulationFactory;

    public void StartServer()
    {
        var gwService = new GWSimulationMultiplexer(_gwSimulationFactory, _logger);

        var server = new Grpc.Core.Server
        {
            Services = {
                GWSimulation.GWSimulation.BindService(gwService)
            },
            Ports = { new ServerPort(_host, _port, ServerCredentials.Insecure) }
        };


        _logger.Information("Starting server on {Host}:{Port}", _host, _port);
        server.Start();
    }
}
