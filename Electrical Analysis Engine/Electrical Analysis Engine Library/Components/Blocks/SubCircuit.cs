using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components.Blocks
{

    /// <summary>
    /// .SUBCKT RES10 1 2 3
    /// R 1 2 10
    /// C1 1 3 1p
    /// C2 2 3 1p
    /// .ENDS
    /// </summary>
    public class SubCircuit:Item
    {

        public SubCircuit()
        {

        }


        public List<Dipole> Components(BasicAnalysis analys = null)
        {
            throw new NotImplementedException();
        }

        public List<NodeSingle> Nodes(BasicAnalysis analys = null)
        {
            throw new NotImplementedException();
        }
    }
}
