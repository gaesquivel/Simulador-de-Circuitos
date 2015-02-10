using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public class Branch: Dipole
    {
        public List<Dipole> BranchComponents { get; protected set; }

        public Branch()
        {
            BranchComponents = new List<Dipole>();
        }


    }
}
