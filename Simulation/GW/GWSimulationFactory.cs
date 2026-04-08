using System.Threading.Tasks;
using Serilog;
using Simulation.Lib.GW;
using Simulation.Services;

namespace Simulation.GW;

public class GWSimulationFactory(
    IDroneSpawner droneSpawner,
    IMapSpawner mapSpawner,
    IPositionUtility positionUtility,
    ILogger logger
) : IGWSimulationFactory
{
    private readonly IDroneSpawner _droneSpawner = droneSpawner;
    private readonly IMapSpawner _mapSpawner = mapSpawner;
    private readonly IPositionUtility _positionUtility = positionUtility;
    private readonly ILogger _logger = logger;

    public Task<IGWSimulation> CreateSimulation(long id)
    {
        var sim = new GWSimulation(_logger, _droneSpawner, _mapSpawner, _positionUtility, id);
        return Task.FromResult<IGWSimulation>(sim);
    }
}
