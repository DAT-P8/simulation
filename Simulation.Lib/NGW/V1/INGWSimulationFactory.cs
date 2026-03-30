namespace Simulation.Lib.NGW.V1;

public interface INGWSimulationFactory
{
    public INGWSimulation CreateSimulation(
        long simulation_id,
        long evader_count,
        long pursuer_count,
        MapConfiguration mapConfiguration
    );
}
