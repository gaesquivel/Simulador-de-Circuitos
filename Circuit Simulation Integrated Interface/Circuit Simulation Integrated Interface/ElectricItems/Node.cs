using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class Node: Item
    {


        public Node(Circuit parent)
            : base()
        {
            Name = "Node" + ID.ToString();
            Parent = parent;
        }

        
    }
}
