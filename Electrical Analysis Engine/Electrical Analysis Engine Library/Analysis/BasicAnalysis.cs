using CircuitMVVMBase;
using ElectricalAnalysis.Analysis.Solver;
using System;

namespace ElectricalAnalysis
{
    public abstract class BasicAnalysis:Item, ICloneable, IDescribible
    {
        public virtual string ShortDescription { get; set; }
        public CircuitSolver Solver { get; protected set; }

        public abstract object Clone();

      

    }
}
