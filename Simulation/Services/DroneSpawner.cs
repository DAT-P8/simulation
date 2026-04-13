using Godot;
using Simulation.Instances;

namespace Simulation.Services;

public class DroneSpawner : IDroneSpawner
{
    private readonly PackedScene _droneDefenderScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly PackedScene _droneAttackerScene = GD.Load<PackedScene>("res://gw_drone_evader.tscn");

    public GWDrone SpawnDrone(int id, int velocity, bool isAttacker)
    {
        PackedScene scene;
        if (isAttacker)
            scene = _droneAttackerScene;
        else
            scene = _droneDefenderScene;

        var drone = scene.Instantiate<StaticBody3D>();
        Main.MainScene.CallDeferred(Node.MethodName.AddChild, drone);

        return new GWDrone(drone, id, isAttacker);
    }
}
