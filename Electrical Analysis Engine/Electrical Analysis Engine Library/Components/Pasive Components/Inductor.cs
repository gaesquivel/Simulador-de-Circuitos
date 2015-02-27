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

        //public override double NortonCurrent(Node referenceNode, double t)
        //{
        //    if (referenceNode == Nodes[0])
        //        return -current.Real;
        //    else
        //        return current.Real;
        //}

        public override double Current(Node referenceNode, double CurrentTime)
        {
            double deltat = CurrentTime - OwnerCircuit.CircuitTime;
            if (deltat <= 0)
            {
                throw new Exception();
            }
            double i = _current.Real + voltage(referenceNode).Real * deltat / Value;
            return i;
        }

        public override Complex32 Current(Node referenceNode, Complex32? W = null)
        {
            return voltage(referenceNode) / Impedance(W);
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
