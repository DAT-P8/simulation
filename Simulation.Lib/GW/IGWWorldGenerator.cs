using Simulation.GridEnvironment;

namespace Simulation.Lib.GW;

public interface IGWWorldGenerator
{
    Task GenerateWorld(GWEnvData envData);
}
