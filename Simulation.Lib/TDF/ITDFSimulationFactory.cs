namespace Simulation.Lib.TDF;

public interface ITDFSimulationFactory
{
    Task<ITDFSimulation> CreateSimulation(long id, int evaders, int pursuers, float attackerDomeRadius, float defenderDomeRadius, float arenaDomeRadius, float drone_max_speed, int seed);
}
