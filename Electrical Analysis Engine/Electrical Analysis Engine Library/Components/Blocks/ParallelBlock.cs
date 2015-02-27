using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{

    /// <summary>
    /// This is a block of parallel conected components
    /// </summary>
    public class ParallelBlock:Block
    {



        public override Complex32 Voltage
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


        public override Complex32 current
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



        //public override Complex32 Impedance(double W = 0)
        //{
        //    Complex32 Y = Complex32.Zero;
        //    Complex32 Z2 = Complex32.Zero;

        //    foreach (var item in Components)
        //    {
        //        Z2 = item.Impedance(W);
        //        if (Z2.IsZero())
        //            return Complex32.PositiveInfinity;
        //        Y += 1 / Z2;
        //    }

        //    return 1 / Y;
        //}


        public override Complex32 Impedance(Complex32 ?W = null)
        {
            Complex32 Y = Complex32.Zero;
            Complex32 Z2 = Complex32.Zero;

            foreach (var item in Components)
            {
                Z2 = item.Impedance(W);
                if (Z2.IsZero())
                    return Complex32.PositiveInfinity;
                Y += 1 / Z2;
            }

            return 1 / Y;
        }

    }
}
