using System;
using Autofac;
using Godot;
using Serilog;
using Simulation.GW;
using Simulation.Lib;
using Simulation.Lib.GW;
using Simulation.Lib.TDF;
using Simulation.TDF;

namespace Simulation.Services;

public static class ServiceConfigurationExtensions
{
    public static ContainerBuilder RegisterServices(this ContainerBuilder builder, ILogger logger, string host, int port, int seed)
    {
        builder.Register(_ => logger).As<ILogger>().SingleInstance();

        builder.RegisterType<DroneSpawner>().As<IDroneSpawner>();
        builder.RegisterType<CameraController>().As<ICameraController>();
        builder.RegisterType<MapSpawner>().As<IMapSpawner>();
        builder.RegisterType<PositionUtility>()
            .WithParameter(TypedParameter.From(new Random(seed)))
            .As<IPositionUtility>();

        builder.RegisterType<GWSimulationFactory>().As<IGWSimulationFactory>().SingleInstance();
        builder.RegisterType<TDFSimulationFactory>().As<ITDFSimulationFactory>().SingleInstance();

        builder.RegisterType<GWSimulationServer>().As<GW2D.V1.SimulationService.SimulationServiceBase>();
        builder.RegisterType<TDFSimulationServer>().As<TDFSimulation.TDFSimulation.TDFSimulationBase>();

        builder.RegisterDecorator<GWLoggingDecorator, GW2D.V1.SimulationService.SimulationServiceBase>();
        builder.RegisterDecorator<GWErrorDecorator, GW2D.V1.SimulationService.SimulationServiceBase>();

        builder.RegisterDecorator<TDFLoggingDecorator, TDFSimulation.TDFSimulation.TDFSimulationBase>();
        builder.RegisterDecorator<TDFErrorDecorator, TDFSimulation.TDFSimulation.TDFSimulationBase>();

        builder.RegisterType<Server>()
            .WithParameter(TypedParameter.From(host))
            .WithParameter(TypedParameter.From(port));

        return builder;
    }
}
