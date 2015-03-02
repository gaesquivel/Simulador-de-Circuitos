using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Inductor : ElectricComponent, PasiveComponent
    {
        double previoustime;
        


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


        public override double Current(Node referenceNode, double t)
        {
            double deltat = t - OwnerCircuit.CircuitTime;
            if (deltat < 0)
            {
                throw new NotImplementedException();
            }
            //si ya se calculo la corriente devuelvo la calculada
            if (previoustime > 0 && previoustime == t)
            {
                if (referenceNode == Nodes[0])
                    return _current.Real;
                else if (referenceNode == Nodes[1])
                    return -_current.Real;
                else
                    throw new NotImplementedException();
            }
            previoustime = t;
            //recalculo la corriente
            double deltai = voltage(referenceNode).Real * deltat / Value;
            double i = _current.Real - deltai;
            if (referenceNode == Nodes[0])
                _current = new Complex32((float)-i, 0);
            else if (referenceNode == Nodes[1])
                _current = new Complex32((float)i, 0);
            else
                throw new NotImplementedException();
            return _current.Real;
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

        public override void Reset()
        {
            previoustime = 0;
            base.Reset();
        }
    }
}
