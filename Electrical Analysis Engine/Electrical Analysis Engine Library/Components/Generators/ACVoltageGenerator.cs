using MathNet.Numerics;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    public class ACVoltageGenerator: VoltageGenerator
    {
        public virtual Complex ACVoltage { get; set; }

        public override Complex Voltage
        {
            get
            {
                return new Complex(Value, 0);
            }
        }

         public ACVoltageGenerator(ComponentContainer owner, string name, string value = null):base(owner)
        {
            Initialize(name, value);
            ACVoltage = new Complex(1, 0);
        }

        public ACVoltageGenerator(ComponentContainer owner, float ACMagnitude = 1, float ACPhase = 0)
            : base(owner)
        {
            ACVoltage = Complex.FromPolarCoordinates(ACMagnitude, ACPhase);
            //ACVoltage = new Complex(ACMagnitude, 0);
        }

        public override Complex voltage(NodeSingle referenceNode, Complex? W = null)
        {
            if (W == null)
            {
                if (referenceNode == Nodes[0])
                    return new Complex(Value, 0);
                return new Complex(-Value, 0);
            }
            else {
                if (referenceNode == Nodes[0])
                    return ACVoltage;
                return ACVoltage;
            
            }
        }

    }
}
