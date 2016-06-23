using System;
using System.Numerics;

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


        public override double Current(NodeSingle referenceNode, double t)
        {
            double deltat = t - previoustime;
            if (deltat < 0)
            {
                throw new NotImplementedException();
            }
            //si ya se calculo la corriente devuelvo la calculada
            if (deltat == 0)
            {
                //if (referenceNode == Nodes[0])
                //    return -_current.Real;
                //else if (referenceNode == Nodes[1])
                return _current.Real;
                //else
                //    throw new NotImplementedException();
            }
            previoustime = t;
            //recalculo la corriente
            double v = voltage(referenceNode, t);
            double deltai = v * deltat / Value;
            double i = _current.Real + deltai;
            //se toma de referencia la tension!!
            //la corriente ya involucra entonces el nodo de referencia

            //if (referenceNode == Nodes[0])
            //    //_current = new Complex(-i, 0);
            //    i = 1 * i;
            //else if (referenceNode == Nodes[1])
            //    //_current = new Complex(i, 0);
            //    i = -i;
            //else
            //    throw new NotImplementedException();

            _current = new Complex(i, 0);
            return _current.Real;
        }

        public override Complex Current(NodeSingle referenceNode, Complex? W = null)
        {
            return voltage(referenceNode) / Impedance(W);
        } 

        public override Complex Impedance(Complex ?W)
        {
            if (W == null)
                return Complex.Zero;
            //jW*L
            //S*L
            Complex L = new Complex( Value, 0);
            return W.Value * L;
        }

        public override void Reset()
        {
            previoustime = 0;
            base.Reset();
        }
    }
}
