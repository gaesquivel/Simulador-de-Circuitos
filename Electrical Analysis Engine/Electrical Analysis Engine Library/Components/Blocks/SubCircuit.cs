using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components.Blocks
{

    /// <summary>
    /// This can contains multiple dipole components, similar to a circuit
    /// but, is a bit more smaller 
    /// .SUBCKT RES10 1 2 3
    /// R 1 2 10
    /// C1 1 3 1p
    /// C2 2 3 1p
    /// .ENDS
    /// </summary>
    public class SubCircuit : Block
    {
        public SubCircuit(ComponentContainer owner) : base(owner)
        {
        }
    }
}
