using Godot;

namespace Simulation.Services;

public class CameraController : ICameraController
{
    private readonly Camera3D _cam;

    public CameraController()
    {
        var children = Main.MainScene.GetChildren();

        Camera3D? camera = null;
        foreach (var c in children)
        {
            if (c is Camera3D cam)
            {
                camera = cam;
                break;
            }
        }

        if (camera is null)
            throw new System.Exception("Did not find the camera of the main scene!");

        _cam = camera;
    }

    public void SetCameraPosition(Vector3 position)
    {
        _cam.CallDeferred(Node3D.MethodName.SetPosition, position);
    }
}
