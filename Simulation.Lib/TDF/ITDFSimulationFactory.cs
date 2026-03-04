namespace Simulation.Lib.TDF;

public interface ITDFSimulationFactory
{
    Task<ITDFSimulation> CreateSimulation();
}
