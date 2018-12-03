using System.Numerics;

namespace ElectricalAnalysis.Components
{
    public class CurrentGenerator : ElectricComponent, Generator
    {
        protected override string DefaultName
        {
            get
            {
                return "I";
            }
        }

        public override Complex current
        {
            get
            {
                return new Complex(Value, 0);
            }
        }

        public CurrentGenerator(ComponentContainer owner)
            : base(owner)
        {
            //Initialize("I" + ID.ToString());
            Expresion = "I";
            Value = 1E-3;
        }

        public CurrentGenerator(ComponentContainer owner, string name, string value)
            : base(owner)
        {
            Initialize(name, value);
        }

        public override Complex Impedance(Complex ?W)
        {
            return double.PositiveInfinity;
        }
        
    }
}
