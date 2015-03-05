using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public class ACVoltageGenerator: VoltageGenerator
    {
        public virtual Complex32 ACVoltage { get; set; }

        public override Complex32 Voltage
        {
            get
            {
                return new Complex32((float)Value, 0);
            }
            //set
            //{
            //    base.Voltage = value;
            //    Value = value.Real;
            //}
        }

         public ACVoltageGenerator(ComponentContainer owner, string name, string value = null):base(owner)
        {
            Initialize(name, value);
            ACVoltage = new Complex32(1, 0);
        }

        public ACVoltageGenerator(ComponentContainer owner, float ACMagnitude = 1, float ACPhase = 0)
            : base(owner)
        {
            ACVoltage = Complex32.FromPolarCoordinates(ACMagnitude, ACPhase);
            //ACVoltage = new Complex32(ACMagnitude, 0);

        }

        public override Complex32 voltage(Node referenceNode, Complex32? W = null)
        {
            if (W == null || W.Value.IsZero())
            {
                if (referenceNode == Nodes[0])
                    return new Complex32((float)Value, 0);
                return new Complex32((float)-Value, 0);
            }
            else {
                if (referenceNode == Nodes[0])
                    return ACVoltage;
                return ACVoltage;
            
            }
        }

    }
}
