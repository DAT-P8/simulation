using System.Threading.Tasks;
using Serilog;
using Simulation.Lib.GW;

namespace Simulation.GridEnvironment;

public class GWSimulationFactory(int mapSize) : IGWSimulationFactory
{
    public Task<IGWSimulation> CreateSimulation()
    {
        return Task.FromResult<IGWSimulation>(new GWSim(Log.Logger, mapSize));
    }
}
