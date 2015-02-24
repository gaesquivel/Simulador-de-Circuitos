using ElectricalAnalysis.Analysis.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis
{
    public class ComplexPlainAnalysis:BasicAnalysis
    {
        public double WMax { get; set; }
        public double WMin { get; set; }
        public double SigmaMax { get; set; }
        public double SigmaMin { get; set; }

        public int Points { get; set; }

        public ComplexPlainAnalysis()
            : base()
        {
            WMax = SigmaMax = 1E4;
            WMin = SigmaMin = -1E4;
            Points = 91;
            Solver = new ComplexPlainSolver();
        }


    }
}
