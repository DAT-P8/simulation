using System.Collections.Generic;
using System.Threading.Tasks;
using Simulation.Lib.TDF;
using TDFSimulation;

namespace Simulation.TDF;

public class TDFSimulation : ITDFSimulation
{
    public Task Close()
    {
        throw new System.NotImplementedException();
    }

    public Task<TDFState> DoStep(List<TDFDroneAction> actions)
    {
        throw new System.NotImplementedException();
    }

    public Task<TDFState> New()
    {
        throw new System.NotImplementedException();
    }

    public Task<TDFState> Reset()
    {
        throw new System.NotImplementedException();
    }
}
