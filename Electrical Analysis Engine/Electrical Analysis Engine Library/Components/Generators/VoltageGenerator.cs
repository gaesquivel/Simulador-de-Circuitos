using System;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    /// <summary>
    /// Represents some kind of Voltage generator
    /// </summary>
    public class VoltageGenerator : ElectricComponent, Generator
    {

        protected override string DefaultName
        {
            get
            {
                return "V";
            }
        }

        public override Complex voltage(NodeSingle ReferenceNode, Complex ?W)
        {
            if (ReferenceNode == Nodes[0])
                return Voltage;
            if (ReferenceNode == Nodes[1])
                return -Voltage;
            return double.NaN;
        }

        public override Complex Voltage
        {
            get
            {
                return new Complex(Value,0);
            }
          
        }

        //public override Complex TheveninVoltage(Node referenceNode, Complex? W = null)
        //{
        //    if (referenceNode == Nodes[0])
        //        return new Complex( Value, 0);
        //    return new Complex(-Value, 0);
        //}


        public VoltageGenerator(ComponentContainer owner)
            : base(owner)
        {
            //Initialize("V" + ID.ToString());
            Expresion = "V";
            Value = 10;
        }

        public VoltageGenerator(ComponentContainer owner, string name, string value):base(owner)
        {
            Initialize(name, value);
        }



    }
}
