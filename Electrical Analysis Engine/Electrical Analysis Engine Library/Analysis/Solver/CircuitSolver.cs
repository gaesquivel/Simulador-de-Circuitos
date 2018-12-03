using ElectricalAnalysis.Analysis.Data;
using ElectricalAnalysis.Components;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public interface CircuitSolver
    {
        DataBase Voltages { get; }
        DataBase Currents { get; }

        bool Solve(Circuit cir, BasicAnalysis ana);
    }
}
