using TDFSimulation;

namespace Simulation.Lib.TDF;

public interface ITDFSimulation
{
    Task<TDFState> New();
    Task<TDFState> Reset();
    Task Close();
    Task<TDFState> DoStep(List<TDFDroneAction> actions);
}
