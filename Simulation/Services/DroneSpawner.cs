using System.Threading.Tasks;
using Godot;
using Simulation.Instances;

namespace Simulation.Services;

public class DroneSpawner : IDroneSpawner
{
    private readonly PackedScene _dronePursuerScene = GD.Load<PackedScene>("res://Resources/gw_drone.tscn");
    private readonly PackedScene _droneEvaderScene = GD.Load<PackedScene>("res://Resources/gw_drone_evader.tscn");

    public async Task<GWDrone> SpawnDroneAsync(int id, bool isEvader)
    {
        PackedScene scene = isEvader ? _droneEvaderScene : _dronePursuerScene;
        var tcs = new TaskCompletionSource<GWDrone>();

        Callable.From(() =>
        {
            var drone = scene.Instantiate<StaticBody2D>();
            Main.MainScene.AddChild(drone);
            tcs.SetResult(new GWDrone(drone, id, isEvader));
        }).CallDeferred();

        return await tcs.Task;
    }
}
