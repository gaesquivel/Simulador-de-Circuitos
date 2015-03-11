using ElectricalAnalysis.Analysis.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis
{
    public class TransientAnalysis:BasicAnalysis
    {
        public string Step { get; set; }
        public string FinalTime { get; set; }

        public TransientAnalysis()
            : base()
        {
            Step = "1u";
            FinalTime = "100u";
            Solver = new TransientSolver();
        }

        public override object Clone()
        {
            TransientAnalysis clon = new TransientAnalysis();
            clon.Step = Step;
            clon.FinalTime = FinalTime;

            return clon;
        }
    }
}
