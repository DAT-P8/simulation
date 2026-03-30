using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Serilog;
using Simulation.Lib.GW;

namespace Simulation.GridEnvironment;

public class GWSimulationFactory() : IGWSimulationFactory
{
    private GWEnvData? envData;
    private int? timeLimit;

    public Task<IGWSimulation> CreateSimulation()
    {
        if (!timeLimit.HasValue || envData == null)
            throw new Exception("SpecifySimulation must be called before a simulation can be created");
        return Task.FromResult<IGWSimulation>(new GWSim(Log.Logger, timeLimit.Value, envData));
    }

    public Task SpecifySimulation(GWEnvData envData, int timeLimit){
        this.envData = envData;
        this.timeLimit = timeLimit;
        return Task.CompletedTask;
    }
}
