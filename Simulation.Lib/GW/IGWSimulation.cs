using GW2D.V1;

namespace Simulation.Lib.GW;

public interface IGWSimulation
{
    Task<State> Reset();
    Task Close();
    Task<State> DoStep(List<DroneAction> actions);
}
