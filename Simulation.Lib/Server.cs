using Grpc.Core;
using Serilog;
using Simulation.Lib.GW;

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
        var gwService = new GWSimulationServer(_gwSimulationFactory, _logger);
        var loggingDecorator = new GWLoggingDecorator(gwService, _logger);
        var errorDecorator = new GWErrorDecorator(loggingDecorator, _logger);

        var server = new Grpc.Core.Server
        {
            Services = {
                GWSimulation.GWSimulation.BindService(errorDecorator)
            },
            Ports = { new ServerPort(_host, _port, ServerCredentials.Insecure) }
        };


        _logger.Information("Starting server on {Host}:{Port}", _host, _port);
        server.Start();
    }
}
