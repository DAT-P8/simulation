using Simulation.Instances;

namespace Simulation.Services;

public interface IDroneSpawner
{
    GWDrone SpawnDrone(int id, int velocity, bool isAttacker);
}
