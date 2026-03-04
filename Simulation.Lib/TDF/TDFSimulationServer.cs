namespace Simulation.Lib.TDF;

public class TDFSimulationServer : GWSimulation.GWSimulation.GWSimulationBase, IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
