using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Inductor : ElectricComponent, PasiveComponent
    {



        public Inductor(ComponentContainer owner)
            : base(owner)
        {
            Initialize("L" + ID.ToString(), "1m");
            //Name = "L" + ID.ToString();
            Expresion = "L";
            //Value = 1E-4;
        }

        public Inductor(ComponentContainer owner, string name, string value)
            : base(owner)
        {
            Initialize(name, value);

        }


        public override Complex32 Impedance(Complex32 ?W)
        {
            if (W == null)
                return Complex32.Zero;
            //jW*L
            //S*L
            Complex32 L = new Complex32((float) Value, 0);
            return W.Value * L;
        }
    }
}
