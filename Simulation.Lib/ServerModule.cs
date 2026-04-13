using Autofac;
using Serilog;
using Simulation.Lib.GW;
using Simulation.Lib.TDF;

namespace Simulation.Lib;

public class ServerModule(
    IGWSimulationFactory gwSimulationFactory,
    ITDFSimulationFactory tdfSimulationFactory,
    ILogger logger,
    string host,
    int port
) : Module
{
    private readonly IGWSimulationFactory _gwSimulationFactory = gwSimulationFactory;
    private readonly ITDFSimulationFactory _tdfSimulationFactory = tdfSimulationFactory;
    private readonly ILogger _logger = logger;
    private readonly string _host = host;
    private readonly int _port = port;

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(_ => _logger).As<ILogger>().SingleInstance();

        builder.Register(c => _gwSimulationFactory).As<IGWSimulationFactory>().SingleInstance();
        builder.Register(c => _tdfSimulationFactory).As<ITDFSimulationFactory>().SingleInstance();

        builder.RegisterType<GWSimulationServer>().As<GW2D.V1.SimulationService.SimulationServiceBase>();
        builder.RegisterType<TDFSimulationServer>().As<TDFSimulation.TDFSimulation.TDFSimulationBase>();

        builder.RegisterDecorator<GWLoggingDecorator, GW2D.V1.SimulationService.SimulationServiceBase>();
        builder.RegisterDecorator<TDFSimulationServer, TDFSimulation.TDFSimulation.TDFSimulationBase>();

        builder.RegisterType<Server>()
            .WithParameter(TypedParameter.From(_host))
            .WithParameter(TypedParameter.From(_port));
    }
}
