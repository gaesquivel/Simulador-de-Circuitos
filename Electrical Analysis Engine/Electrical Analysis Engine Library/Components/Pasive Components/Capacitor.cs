using MathNet.Numerics;
using System;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    public class Capacitor: ElectricComponent, PasiveComponent
    {
        /// <summary>
        /// cargas almacenadas en el capacitor
        /// </summary>
        double charge = 0;

        double previoustime;

        public override double voltage(NodeSingle referenceNode, double t)
        {
            double deltat = t - previoustime;
            if (deltat < 0)
            {
                throw new Exception();
            }
            //si ya se calculo la corriente devuelvo la calculada
            if (deltat == 0)
            {
                if (referenceNode == Nodes[0])
                    return -charge / Value;
                else if (referenceNode == Nodes[1])
                    return charge / Value;
                else
                    throw new NotImplementedException();
            }
            previoustime = t;
            //recalculo la corriente
            double i = 0;
           
            i = Current(referenceNode, t);
            //double vant = charge / Value;
            //if (referenceNode == Nodes[0])
            //    vant = -vant;
            double q = i * deltat;
            charge += q;
            double v = charge / Value;

            //if (referenceNode == Nodes[0])
                return v;
            //else if (referenceNode == Nodes[1])
            //    return -v;
            //else
            //    throw new InvalidOperationException();
        }

        public override Complex Current(NodeSingle referenceNode, Complex? W = null)
        {
            return voltage(referenceNode) / Impedance(W);
        }

        public Capacitor(ComponentContainer owner)
            : base(owner)
        {
            Initialize("C" + ID.ToString());
            Expresion = "C";
            Value = 1E-6;
        }

        public Capacitor(ComponentContainer owner, string name, string value)
            : base(owner)
        {
            Initialize(name, value);

        }

        public override Complex Impedance(Complex? W = null)
        {
            if (W == null || W.Value.IsZero())
                return Double.PositiveInfinity;
            // 1/jWC
            // 1/SC
            return (W.Value * new Complex(Value,0)).Reciprocal();
        }

        public override void Reset()
        {
            charge = 0;
            previoustime = 0;
            base.Reset();
        }

    }
}
