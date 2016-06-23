using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class SuperNode:Node
    {
        /// <summary>
        /// All nodes members of the supernode
        /// </summary>
        public List<NodeSingle> Nodes { get; protected set; }

        protected override string DefaultName { get { return "SuperNode"; } }


        public SuperNode()
        {
            Nodes = new List<NodeSingle>();
            //FindSuperNodeElements(FirstNode);
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            string s = "";
            foreach (var nodo in Nodes)
            {
                s += nodo.ToString() + ", ";
            }
            return Name + ": " + s;
        }

    }
}
