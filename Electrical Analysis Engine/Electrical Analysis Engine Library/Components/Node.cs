using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    public class Node: Item
    {

        public Complex Voltage { get; internal set; }
        public bool IsReference { get; set; }
        public List<ElectricComponent> Components { get; protected set; }
        
        /// <summary>
        /// indica si el nodo esta conectado directamente a un generador de tension
        /// </summary>
        public bool IsVoltageConnected
        {
            get {
                foreach (var item in Components)
                {
                    if (item is VoltageGenerator)
                        return true;
                }
                return false;
            }
        }


        //Circuit parent
        public Node(string name = null)
            : base()
        {
            Components = new List<ElectricComponent>();
            if (string.IsNullOrEmpty(name))
                Name = "Node" + ID.ToString();
            else
                Name = name;
            //Parent = parent;
        }

        
    }
}
