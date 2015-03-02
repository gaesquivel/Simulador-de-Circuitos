using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Resistor : ElectricComponent, PasiveComponent
    {

        public override double Current(Node referenceNode, double CurrentTime)
        {
            Complex32 i = Voltage / Impedance();
            if (referenceNode == Nodes[0])
                return i.Real;
            else
                return -i.Real;
        }

        public override Complex32 current
        {
            get
            {
                //return new Complex32((float)((Nodes[0].Voltage.Real - Nodes[1].Voltage.Real) / Value), 0);
                return Voltage / Impedance();
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

        public override Complex32 Impedance(Complex32 ?W = null)
        {
            return new Complex32((float)Value, 0);
        }

        //public override Complex32 Impedance(double W = 0)
        //{
        //    return new Complex32((float)Value, 0);
        //}

    }
}
