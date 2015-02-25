using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public class VoltageGenerator : ElectricComponent, Generator
    {

        public override Complex32 voltage(Node ReferenceNode)
        {
            if (ReferenceNode == Nodes[0])
                return Voltage;
            if (ReferenceNode == Nodes[1])
                return -Voltage;
            return Complex32.NaN;
        }

        public override Complex32 Voltage
        {
            get
            {
                return new Complex32((float)Value,0);
            }
          
        }

        public override Complex32 TheveninVoltage(Node referenceNode, Complex32? W = null)
        {
            if (referenceNode == Nodes[0])
                return new Complex32((float) Value, 0);
            return new Complex32((float)-Value, 0);
        }


        public VoltageGenerator(ComponentContainer owner)
            : base(owner)
        {
            Initialize("V" + ID.ToString());
            Expresion = "V";
            Value = 10;
        }

        public VoltageGenerator(ComponentContainer owner, string name, string value):base(owner)
        {
            Initialize(name, value);
        }



    }
}
