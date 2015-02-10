using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Inductor:PasiveComponent
    {

        public override System.Numerics.Complex Impedance(double W)
        {
            return new System.Numerics.Complex(0, W * Value);
        }

        public Inductor()
            : base()
        {
            Initialize("L" + ID.ToString(), "1m");
            //Name = "L" + ID.ToString();
            Expresion = "L";
            //Value = 1E-4;
        }

        public Inductor(string name, string value)
            : base()
        {
            Initialize(name, value);

        }

    }
}
