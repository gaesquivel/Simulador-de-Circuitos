using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components.Controlled
{
    public class VoltageControlledGenerator:VoltageGenerator, ControlledVoltageGenerator, ControllerOpenCircuit
    {
        double gain;
        bool wasparsed;

        public List<Node> InputNodes { get; protected set; }
        public string Gain { get; set; }

        public VoltageControlledGenerator(ComponentContainer owner, string name)
            : base(owner)
        {
            InputNodes = new List<Node>();
            Initialize(name);
            Gain = "10";
        }

        public override MathNet.Numerics.Complex32 voltage(Node referenceNode, MathNet.Numerics.Complex32? W = null)
        {
            Complex32 v = InputNodes[0].Voltage;
            if (InputNodes.Count > 1)
            {
                //si no esta una entrada a tierra hay que hacer la diferencia
                v -= InputNodes[1].Voltage;
            }
            v = v * new Complex32((float)gain, 0);
            if (referenceNode == Nodes[0])
                return v;
            else if (referenceNode == Nodes[1])
                return -v;
            else
                return Complex32.NaN;
        }

        public override double voltage(Node referenceNode, double t)
        {
            if (!wasparsed)
            {
                gain = StringUtils.DecodeString(Gain);
                wasparsed = true;
            }
            double v = InputNodes[0].Voltage.Real;
            if (InputNodes.Count > 1)
            {
                //si no esta una entrada a tierra hay que hacer la diferencia
                v -= InputNodes[1].Voltage.Real;
            }
            v = v * gain;
            if (referenceNode == Nodes[0])
                return v;
            else if (referenceNode == Nodes[1])
                return -v;
            else
                return double.NaN;
            //return base.voltage(referenceNode, t);
        }

        public override void Reset()
        {
            wasparsed = false;
            base.Reset();
        }

    }
}
