using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Capacitor: ElectricComponent, PasiveComponent
    {
        /// <summary>
        /// tension instantanea del capacitor
        /// </summary>
        double _voltage = 0;
        double previoustime;

        public override Complex32 current
        {
            get
            {
                return _current; ;
            }
            internal set
            {
                _current = value;
            }
        }

       

        //public override double TheveninVoltage(Node referenceNode, double t)
        //{
        //    return voltage(referenceNode, t);
        //}

        public override double voltage(Node referenceNode, double t)
        {
            double deltat = t - OwnerCircuit.CircuitTime;
            if (deltat < 0)
            {
                throw new Exception();
            }
            //si ya se calculo la corriente devuelvo la calculada
            if (previoustime > 0 && previoustime == t)
            {
                if (referenceNode == Nodes[0])
                    return _voltage;
                else if (referenceNode == Nodes[1])
                    return -_voltage;
                else
                    throw new NotImplementedException();
            }
            previoustime = t;
            //recalculo la corriente
            double i = 0;
            if (Owner is Branch)
            {
                i = ((Branch)Owner).Current(referenceNode, t);
            }
            else
                i = Current(referenceNode, t);
            double vant = 0;
            if (referenceNode == Nodes[0])
                vant = -_voltage;
            else if (referenceNode == Nodes[1])
                vant = _voltage;
            
            double v = vant + i * deltat / Value;
            _voltage = v;
            return v;
        }

        public override Complex32 Current(Node referenceNode, Complex32? W = null)
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

        public override Complex32 Impedance(Complex32? W = null)
        {
            if (W == null || W.Value.IsZero())
                return Complex32.PositiveInfinity;
            // 1/jWC
            // 1/SC
            return (W.Value * new Complex32((float)Value,0)).Reciprocal();
        }

        public override void Reset()
        {
            _voltage = 0;
            previoustime = 0;
            base.Reset();
        }

    }
}
