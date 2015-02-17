using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public interface CircuitSolver
    {

        bool Solve(Circuit cir, BasicAnalysis ana);
    }
}
