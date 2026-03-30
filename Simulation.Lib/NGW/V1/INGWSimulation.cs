using GW2D.V1;

namespace Simulation.Lib.NGW.V1;

public interface INGWSimulation
{
    NGWState GetState();
    void Close();
    void DoStep(List<NGWDroneAction> actions);
    void Reset();
}
