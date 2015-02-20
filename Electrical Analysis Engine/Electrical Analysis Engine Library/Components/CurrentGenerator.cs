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

        public CurrentGenerator(ComponentContainer owner)
            : base(owner)
        {
            Initialize("I" + ID.ToString());
            Expresion = "I";
            Value = 1E-3;
        }

        public CurrentGenerator(ComponentContainer owner, string name, string value)
            : base(owner)
        {
            Initialize(name, value);
        }

        public override Complex32 Impedance(Complex32 ?W)
        {
            return Complex32.PositiveInfinity;
        }
        
    }
}
