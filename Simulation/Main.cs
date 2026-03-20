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
	private const int mapSize = 8;

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

		GWEnvData envData = new(mapSize);
		var gwFactory = new GWSimulationFactory(envData);
		var tdfFactory = new TDFSimulationFactory();
		var world = new GWMap(envData);
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
