using System.Threading.Tasks;
using Serilog;
using Simulation.Lib.GW;

namespace Simulation.GridEnvironment;

public class GWSimulationFactory : IGWSimulationFactory
{
    public Task<IGWSimulation> CreateSimulation()
    {
        return Task.FromResult<IGWSimulation>(new GWSim(Log.Logger));
    }
}
