using System.Numerics;

namespace ElectricalAnalysis.Components
{
    public class Resistor : ElectricComponent, PasiveComponent
    {

        public override double Current(NodeSingle referenceNode, double CurrentTime)
        {
            Complex i = Voltage / Impedance();
            if (referenceNode == Nodes[0])
                return i.Real;
            else
                return -i.Real;
        }

        public override Complex current
        {
            get
            {
                //return new Complex(((Nodes[0].Voltage.Real - Nodes[1].Voltage.Real) / Value), 0);
                return Voltage / Impedance();
            }
            //internal set
            //{
            //    _current = value;
            //}
        }

        public override Complex Current(NodeSingle referenceNode, Complex? W = null)
        {
            Complex i = Voltage / Impedance(W);
            if (referenceNode == Nodes[0])
                return i;
            else
                return -i;
        }

        public Resistor(ComponentContainer owner)
            : base(owner)
        {
            Initialize("R" + ID.ToString());
            //Name = "R" + ID.ToString();
            Expresion = "R";
            Value = 1000;
        }

        public Resistor(ComponentContainer owner, string name, string value):base(owner)
        {
            Initialize(name, value);

        }

        public override Complex Impedance(Complex ?W = null)
        {
            return new Complex(Value, 0);
        }


        public override Complex voltage(NodeSingle ReferenceNode, Complex? W = null)
        {
            return 0;
        }

    }
}
