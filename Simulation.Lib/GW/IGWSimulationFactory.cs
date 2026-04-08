namespace Simulation.Lib.GW;

public interface IGWSimulationFactory
{
    Task<IGWSimulation> CreateSimulation(long id);
}
