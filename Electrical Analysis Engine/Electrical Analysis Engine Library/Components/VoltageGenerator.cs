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

        public override Complex32 Voltage
        {
            get
            {
                return new Complex32((float)Value,0);
            }
            set
            {
                base.Voltage = value;
                Value = value.Real;
            }
        }

        public override double TheveninVoltage(Node referenceNode)
        {
            if (referenceNode == Nodes[0])
                return Value;
            return -Value;
        }


        public VoltageGenerator()
            : base()
        {
            Initialize("V" + ID.ToString());
            Expresion = "V";
            Value = 10;
        }

        public VoltageGenerator(string name, string value):base()
        {
            Initialize(name, value);
        }



    }
}
