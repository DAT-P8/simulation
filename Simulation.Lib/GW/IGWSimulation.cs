using GWSimulation;

namespace Simulation.Lib.GW;

public interface IGWSimulation
{
    Task<GWState> Reset();
    Task Close();
    Task<GWState> DoStep(List<GWDroneAction> actions);
}
