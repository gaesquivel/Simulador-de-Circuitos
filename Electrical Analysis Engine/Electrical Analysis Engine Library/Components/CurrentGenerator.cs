using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public class CurrentGenerator : ElectricComponent, Generator
    {
        public override Complex32 Current
        {
            get
            {
                return new Complex32((float)Value, 0);
            }
            internal set
            {
                base.Current = value;
            }
        }

        public CurrentGenerator()
            : base()
        {
            Initialize("I" + ID.ToString());
            Expresion = "I";
            Value = 1E-3;
        }

        public CurrentGenerator(string name, string value)
            : base()
        {
            Initialize(name, value);
        }

        public override Complex32 Impedance(double W)
        {
            return Complex32.PositiveInfinity;
        }
        
    }
}
