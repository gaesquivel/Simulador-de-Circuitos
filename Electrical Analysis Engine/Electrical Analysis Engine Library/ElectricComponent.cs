using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class ElectricComponent:Item
    {

        public Complex Current { get; internal set; }

        //Expresion R, L, C
        public string Expresion { get; set; }   //model?
        public double Value { get; set; }   //1000 (ohms, Henry...)

        /// <summary>
        /// Listado de nodos de un componente. Normalmente son 2 nada mas
        /// </summary>
        public List<Node> Nodes { get; protected set; }

        public ElectricComponent():base()   
        {
            Nodes = new List<Node>();
            Name = "Component" + ID.ToString();

            Nodes.Add(new Node());
            Nodes.Add(new Node());
        }



    }
}
