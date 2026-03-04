using System.Threading.Tasks;
using Simulation.Lib.TDF;

namespace Simulation.TDF;

public class TDFSimulationFactory : ITDFSimulationFactory
{
    public Task<ITDFSimulation> CreateSimulation()
    {
        throw new System.NotImplementedException();
    }
}
