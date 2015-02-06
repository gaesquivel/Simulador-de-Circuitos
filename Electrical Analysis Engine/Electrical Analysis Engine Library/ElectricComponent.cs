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

        //public ElectricComponent(string name = null, string value)
        //    : base()
        //{
        //    this.ElectricComponent(name, 0);
        //}

        public ElectricComponent()
            : base()
        { 
            Nodes = new List<Node>();
            Nodes.Add(new Node());
            Nodes.Add(new Node());
        }

        protected void Initialize(string name = null, string value = null)
        { 
            if (string.IsNullOrEmpty(name))
                Name = "Component" + ID.ToString();
            else
                Name = name;

            if (string.IsNullOrEmpty(value))
                Value = 0;
            else
            {
                Value = StringUtils.DecodeString(value);

            }
        }


    }
}
