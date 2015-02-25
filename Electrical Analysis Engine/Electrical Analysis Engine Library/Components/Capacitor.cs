using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Capacitor: ElectricComponent, PasiveComponent
    {

        public override Complex32 current
        {
            get
            {
                return Complex32.Zero;
            }
            internal set
            {
                _current = value;
            }
        }

        public override Complex32 Current(Node referenceNode, Complex32? W = null)
        {
            Complex32 i = Voltage / Impedance(W);
            if (referenceNode == Nodes[0])
                return i;
            else
                return -i;
        }

        public Capacitor(ComponentContainer owner)
            : base(owner)
        {
            Initialize("C" + ID.ToString());
            Expresion = "C";
            Value = 1E-6;
        }

        public Capacitor(ComponentContainer owner, string name, string value)
            : base(owner)
        {
            Initialize(name, value);

        }

        public override Complex32 Impedance(Complex32? W = null)
        {
            if (W == null || W.Value.IsZero())
                return Complex32.PositiveInfinity;
            // 1/jWC
            // 1/SC
            return (W.Value * new Complex32((float)Value,0)).Reciprocal();
        }
    }
}
