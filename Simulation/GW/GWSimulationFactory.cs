using System.Threading.Tasks;
using Simulation.Lib.GW;

namespace Simulation.GW;

public class GWSimulationFactory : IGWSimulationFactory
{
    public Task<IGWSimulation> CreateSimulation()
    {
        throw new System.NotImplementedException();
    }
}
