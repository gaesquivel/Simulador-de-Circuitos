using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Inductor : ElectricComponent, PasiveComponent
    {

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


        public override Complex32 Impedance(double W)
        {
            return new Complex32(0, (float)(W * Value));
        }
    }
}
