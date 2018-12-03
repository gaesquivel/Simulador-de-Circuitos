using ElectricalAnalysis.Analysis.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis.Components;

namespace ElectricalAnalysis.Analysis.Solver
{
    public abstract class BasicSolver: CircuitSolver
    {
        public virtual DataBase Voltages { get; protected set; }
        public virtual DataBase Currents { get; protected set; }

        public abstract bool Solve(Circuit cir, BasicAnalysis ana);
    }
}
