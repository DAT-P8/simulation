using Simulation.GridEnvironment;

namespace Simulation.Lib.GW;

public interface IGWSimulationFactory
{
    Task<IGWSimulation> CreateSimulation();
    Task SpecifySimulation(GWEnvData envData, int timeLimit);
}
