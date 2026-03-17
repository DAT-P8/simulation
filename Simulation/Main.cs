using System;
using Godot;
using Serilog;
using Simulation.GridEnvironment;
using Simulation.Lib;
using Simulation.TDF;

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
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        Log.Logger = logger;

        var gwFactory = new GWSimulationFactory();
        var tdfFactory = new TDFSimulationFactory();
        var world = new GWMap(10);
        var view = world.GenerateTexture();
        AddChild(view);
        AddChild(world.ConstructMap(view));

        var server = new Server(logger, "localhost", 50051, gwFactory, tdfFactory);
        server.StartServer();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
