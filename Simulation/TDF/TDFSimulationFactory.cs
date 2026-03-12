using System.Threading.Tasks;
using Serilog;
using Simulation.Lib.TDF;

namespace Simulation.TDF;

public class TDFSimulationFactory : ITDFSimulationFactory
{
    public Task<ITDFSimulation> CreateSimulation(long id, int evaders, int pursuers, float attackerDomeRadius, float defenderDomeRadius, float arenaDomeRadius, float drone_max_speed, int seed)
    {
        return Task.FromResult<ITDFSimulation>(new TDFSimulation(
            Log.Logger,
            id,
            evaders,
            pursuers,
            attackerDomeRadius,
            defenderDomeRadius,
            arenaDomeRadius,
            drone_max_speed,
            seed
        ));
    }
}
