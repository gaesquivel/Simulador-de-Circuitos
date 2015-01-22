using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class ElectricComponent:Item
    {
        /// <summary>
        /// Listado de nodos de un componente. Normalmente son 2 nada mas
        /// </summary>
        public List<Pin> Nodes { get; protected set; }

        public ElectricComponent():base()   
        {
            Nodes = new List<Pin>();
            Name = "Component" + ID.ToString();
            Nodes.Add(new Pin(this));
            Nodes.Add(new Pin(this));
        }



    }
}
