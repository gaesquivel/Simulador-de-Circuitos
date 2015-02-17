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
            //Name = "C" + ID.ToString();
            Expresion = "C";
            Value = 1E-6;
        }

        public Capacitor(string name, string value)
            : base()
        {
            Initialize(name, value);

        }

        public override Complex32 Impedance(double W)
        {
            if (W>0)
                return new Complex32(0, (float)(1 / W * Value));
            return Complex32.NaN;
        }
    }
}
