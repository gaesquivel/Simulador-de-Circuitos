using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class Pin:Item
    {

        public Node node { get; set; }

        public Pin(ElectricComponent parent)
            : base()
        {
            Name = "Node" + ID.ToString();
            Parent = parent;
        }

    }


}
