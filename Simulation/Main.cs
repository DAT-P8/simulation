using Godot;
using Serilog;
using Simulation.GridEnvironment;
using Simulation.Lib;

namespace Simulation;


public partial class Main : Node3D
{
	public static Main Scene { get; private set; }

	public Main() {
		Scene = this;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();
		Log.Logger = logger;

        var world = new GWMap(10);
        var view = world.GenerateTexture();
        AddChild(view);
        AddChild(world.ConstructMap(view));

		var gwFactory = new GWSimulationFactory();
		var server = new Server(logger, "localhost", 50051, gwFactory);
		server.StartServer();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
