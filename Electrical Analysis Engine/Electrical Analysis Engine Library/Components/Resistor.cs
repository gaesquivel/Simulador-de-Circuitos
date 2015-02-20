using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Resistor : ElectricComponent, PasiveComponent
    {
        public override Complex32 Current
        {
            get
            {
                return new Complex32((float)((Nodes[0].Voltage.Real - Nodes[1].Voltage.Real) / Value), 0);
            }
            internal set
            {
                base.Current = value;
            }
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
