using MathNet.Numerics;
using System;
using System.Numerics;

namespace ElectricalAnalysis.Components
{

    /// <summary>
    /// This is a block of parallel conected components
    /// </summary>
    public class ParallelBlock:Block
    {



        public override Complex Voltage
        {
            get
            {
                foreach (var item in Components)
                {
                    if (item is VoltageGenerator)
                    {
                        return item.Voltage;
                    }
                }
                return base.Voltage;
            }
            //set
            //{
            //    base.Voltage = value;
            //}
        }


        public override Complex current
        {
            get
            {
                return Voltage / Impedance();
            }
            internal set
            {
                _current = value;
            }
        }


        public ParallelBlock(ComponentContainer owner, Dipole comp1, Dipole comp2):base(owner)
        {
            Components.Add(comp1);
            Components.Add(comp2);
            Nodes.Add(comp1.Nodes[0]);
            Nodes.Add(comp1.Nodes[1]);
        }



        //public override Complex Impedance(double W = 0)
        //{
        //    Complex Y = Complex.Zero;
        //    Complex Z2 = Complex.Zero;

        //    foreach (var item in Components)
        //    {
        //        Z2 = item.Impedance(W);
        //        if (Z2.IsZero())
        //            return Complex.PositiveInfinity;
        //        Y += 1 / Z2;
        //    }

        //    return 1 / Y;
        //}


        public override Complex Impedance(Complex ?W = null)
        {
            Complex Y = Complex.Zero;
            Complex Z2 = Complex.Zero;

            foreach (var item in Components)
            {
                Z2 = item.Impedance(W);
                if (Z2.IsZero())
                    return Double.PositiveInfinity;
                Y += 1 / Z2;
            }

            return 1 / Y;
        }

    }
}
