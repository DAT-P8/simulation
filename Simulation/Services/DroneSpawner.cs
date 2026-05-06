using System.Threading.Tasks;
using Godot;
using Simulation.Instances;

namespace Simulation.Services;

public class DroneSpawner : IDroneSpawner
{
    private readonly PackedScene _droneDefenderScene = GD.Load<PackedScene>("res://gw_drone.tscn");
    private readonly PackedScene _droneAttackerScene = GD.Load<PackedScene>("res://gw_drone_evader.tscn");


    public async Task<GWDrone> SpawnDroneAsync(int id, bool isAttacker)
    {
        PackedScene scene = isAttacker ? _droneAttackerScene : _droneDefenderScene;
        var tcs = new TaskCompletionSource<GWDrone>();

        Callable.From(() =>
        {
            var drone = scene.Instantiate<StaticBody3D>();
            Main.MainScene.AddChild(drone);
            tcs.SetResult(new GWDrone(drone, id, isAttacker));
        }).CallDeferred();

        return await tcs.Task;
    }
}
