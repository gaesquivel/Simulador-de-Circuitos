using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Capacitor: ElectricComponent, PasiveComponent
    {
        public Capacitor()
            : base()
        {
            Initialize("C" + ID.ToString());
            Expresion = "C";
            Value = 1E-6;
        }

        public Capacitor(string name, string value)
            : base()
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
