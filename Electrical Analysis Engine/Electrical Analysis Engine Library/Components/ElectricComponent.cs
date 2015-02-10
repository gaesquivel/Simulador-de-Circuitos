using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public abstract class ElectricComponent:Item
    {

        public double Temperature { get; set; }
        public abstract Complex Impedance(double W);
        

        /// <summary>
        /// Valor de la corriente de continua
        /// </summary>
        public Complex Current { 
            get
            {
                if (this is Capacitor)
                    return 0;
                if (this is Resistor)
                        return new Complex((Nodes[0].Voltage.Real - Nodes[1].Voltage.Real) / Value, 0);
                if (this is CurrentGenerator)
                    return Value;

                if (this is Inductor)
                    return 0;
                if (this is VoltageGenerator)
                {
                    
                }
                return Complex.Zero;
            } 
            internal set; }

        //Expresion R, L, C
        public string Expresion { get; set; }   //model?
        public double Value { get; set; }   //1000 (ohms, Henry...)

        /// <summary>
        /// Listado de nodos de un componente. Normalmente son 2 nada mas
        /// </summary>
        public List<Node> Nodes { get; protected set; }

     

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
