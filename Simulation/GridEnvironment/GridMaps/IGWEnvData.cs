using Godot;

namespace Simulation.GridEnvironment.GridMaps;

public interface IGWEnvData{
    Vector3 GetMapPosition();
    (int, int) GetTargetPosition();
    bool IsInTarget(int x,int y);
    bool IsInBounds(int x,int y);
}
