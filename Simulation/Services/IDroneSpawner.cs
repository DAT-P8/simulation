using System.Threading.Tasks;
using Simulation.Instances;

namespace Simulation.Services;

public interface IDroneSpawner
{
    Task<GWDrone> SpawnDroneAsync(int id, bool isEvader);
}
