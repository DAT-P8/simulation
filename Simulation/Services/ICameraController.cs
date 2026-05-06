using Godot;
using GW2D.V1;

namespace Simulation.Services;

public interface ICameraController
{
    void SetCameraPosition(Vector2 position, SquareMap squareMap);
}
