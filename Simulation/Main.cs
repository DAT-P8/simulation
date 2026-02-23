using Godot;
using Serilog;
using Simulation.GridEnvironment;
using Simulation.Lib;

namespace Simulation;


public partial class Main : Node3D
{
	private readonly PackedScene _droneScene = GD.Load<PackedScene>("res://gw_drone.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();
		Log.Logger = logger;

		var gwService = new GWService(logger);

		var server = new Server(logger, "localhost", 50051, gwService);
		server.StartServer();

		var drone = _droneScene.Instantiate<StaticBody3D>();
		AddChild(drone);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
