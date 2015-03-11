using ElectricalAnalysis.Analysis.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class DCAnalysis: BasicAnalysis
    {

        public DCAnalysis()
            : base()
        {

            Solver = new DCSolver();
        }

        public override object Clone()
        {
            return new DCAnalysis();
        }

    }
}
