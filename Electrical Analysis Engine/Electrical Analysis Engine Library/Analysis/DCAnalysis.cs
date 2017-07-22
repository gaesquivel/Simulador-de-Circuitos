using System;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;

namespace ElectricalAnalysis
{
    public class DCAnalysis: BasicAnalysis
    {
        protected override string DefaultName { get { return "DC Bias Analysis"; } }

        public DCAnalysis()
            : base()
        {

            Solver = new DCSolver();
        }

        public override object Clone()
        {
            return new DCAnalysis();
        }

        public override bool Parse(Circuit owner, string details)
        {
            throw new NotImplementedException();
        }

    }
}
