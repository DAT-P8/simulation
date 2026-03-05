using Grpc.Core;
using Serilog;
using Simulation.Lib.GW;
using Simulation.Lib.TDF;

namespace Simulation.Lib;

public class Server(
    ILogger logger,
    string host,
    int port,
    IGWSimulationFactory gwSimulationFactory,
    ITDFSimulationFactory tdfSimulationFactory
)
{
    private readonly ILogger _logger = logger;
    private readonly string _host = host;
    private readonly int _port = port;
    private readonly IGWSimulationFactory _gwSimulationFactory = gwSimulationFactory;
    private readonly ITDFSimulationFactory _tdfSimulationFactory = tdfSimulationFactory;

    public void StartServer()
    {
        var gwService = new GWSimulationServer(_gwSimulationFactory, _logger);
        var gwLoggingDecorator = new GWLoggingDecorator(gwService, _logger);
        var gwErrorDecorator = new GWErrorDecorator(gwLoggingDecorator, _logger);

        var tdfService = new TDFSimulationServer(_tdfSimulationFactory, _logger);
        var tdfLoggingDecorator = new TDFLoggingDecorator(tdfService, _logger);
        var tdfErrorDecorator = new TDFErrorDecorator(tdfLoggingDecorator, _logger);

        var server = new Grpc.Core.Server
        {
            Services = {
                GWSimulation.GWSimulation.BindService(gwErrorDecorator),
                TDFSimulation.TDFSimulation.BindService(tdfErrorDecorator)
            },
            Ports = { new ServerPort(_host, _port, ServerCredentials.Insecure) }
        };


        _logger.Information("Starting server on {Host}:{Port}", _host, _port);
        server.Start();
    }
}
