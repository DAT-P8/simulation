using System;
using System.Threading.Tasks;
using Simulation.Lib.TDF;

namespace Simulation.TDF;

public class TDFSimulationFactory : ITDFSimulationFactory
{
    private const float DRONE_MAX_SPEED = 10;
    private readonly Random _random = new();

    public Task<ITDFSimulation> CreateSimulation(long id, int evaders, int pursuers, float attackerDomeRadius, float defenderDomeRadius, float arenaDomeRadius)
    {
        return Task.FromResult<ITDFSimulation>(new TDFSimulation(
            id,
            evaders,
            pursuers,
            attackerDomeRadius,
            defenderDomeRadius,
            arenaDomeRadius,
            DRONE_MAX_SPEED,
            _random.Next())
        );
    }
}
