using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class Circuit:Item
    {

        public List<ElectricComponent> Components { get; protected set; }
        public List<Node> Nodes { get; protected set; }

        public Circuit()
            : base()
        {
            Name = "Circuit" + ID.ToString();
            Components = new List<ElectricComponent>();
            Nodes = new List<Node>();
        }


    }
}
