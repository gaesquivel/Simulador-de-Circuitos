using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ElectricalAnalysis
{
    public class Node: Item
    {

        public Complex Voltage { get; internal set; }
        //Circuit parent
        public Node()
            : base()
        {
            Name = "Node" + ID.ToString();
            //Parent = parent;
        }

        
    }
}
