using System.Threading.Tasks;
using Serilog;
using Simulation.Lib.GW;
using Simulation.GridEnvironment.GridMaps;

namespace Simulation.GridEnvironment;

public class GWSimulationFactory(IGWEnvData envData) : IGWSimulationFactory
{
    public Task<IGWSimulation> CreateSimulation()
    {
        return Task.FromResult<IGWSimulation>(new GWSim(Log.Logger, envData));
    }
}
