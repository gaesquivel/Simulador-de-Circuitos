using ElectricalAnalysis.Components;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public interface CircuitSolver
    {

        bool Solve(Circuit cir, BasicAnalysis ana);
    }
}
