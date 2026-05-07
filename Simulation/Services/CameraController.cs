using Godot;
using GW2D.V1;
using Simulation.Utils;

namespace Simulation.Services;

public class CameraController : ICameraController
{
    private readonly Camera2D _cam = SceneObjectGetter.GetObject<Camera2D>();

    public void SetCameraPosition(Vector2 position, SquareMap squareMap)
    {
        _cam.CallDeferred(Camera2D.MethodName.SetZoom, new Vector2(2f, 2f));
        _cam.CallDeferred(Node2D.MethodName.SetPosition, position);
    }
}

