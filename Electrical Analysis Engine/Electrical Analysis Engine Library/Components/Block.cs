using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{

    /// <summary>
    /// Represent a generic block of conected components
    /// </summary>
    public class Block: Dipole
    {
        public List<Dipole> Components { get; protected set; }
        //public List<Node> ExternalNodes { get; protected set; }

        public Block():base()
        {
            Components = new List<Dipole>();
            //ExternalNodes = new List<Node>();
        }


    }
}
