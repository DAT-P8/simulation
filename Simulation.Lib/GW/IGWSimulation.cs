using GW2D.V1;

namespace Simulation.Lib.GW;

public interface IGWSimulation
{
    Task<State> Reset();
    Task Close();
    Task<State> DoStep(List<DroneAction> actions);
    Task<State> New(MapSpec mapSpec, int evaders, int pursuers);
}
