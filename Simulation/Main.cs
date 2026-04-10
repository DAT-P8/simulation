using System;
using Godot;
using Serilog;
using Serilog.Events;
using Simulation.Lib;
using Simulation.TDF;
using Autofac;
using Simulation.GW;
using Simulation.Lib.GW;
using Simulation.Lib.TDF;
using Simulation.Services;

namespace Simulation;


public partial class Main : Node3D
{
    public static Main MainScene { get; internal set; } = null!;

    public Main()
    {
        if (MainScene != null) throw new Exception("MainScene is already constructed!");
        MainScene = this;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var logLevel = RetrieveLogLevelFromArgs();
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Is(logLevel)
            .CreateLogger();
        Log.Logger = logger;

        var builder = new ContainerBuilder();
        builder.RegisterServices(logger, "localhost", 50051, 420);
        var container = builder.Build();

        var server = container.Resolve<Server>();
        server.StartServer();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private LogEventLevel RetrieveLogLevelFromArgs()
    {
        var logLevel = LogEventLevel.Error; // default
        var args = OS.GetCmdlineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--log-level" && i + 1 < args.Length)
            {
                if (Enum.TryParse<LogEventLevel>(args[i + 1], ignoreCase: true, out var parsed))
                    logLevel = parsed;
                break;
            }
        }
        return logLevel;
    }
}
