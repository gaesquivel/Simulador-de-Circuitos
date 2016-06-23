using System;
using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Components.Controlled
{
    public class VoltageControlledGenerator:VoltageGenerator, ControlledVoltageGenerator, 
        ControllerOpenCircuit
    {
        protected double gain;
        //bool wasparsed;
        string _gain;

        protected override string DefaultName { get { return "Av"; } }
        public List<NodeSingle> InputNodes { get; protected set; }

        /// <summary>
        /// DC, AC & Transient constant Voltage Gain
        /// </summary>
        public string Gain
        {
            get { return _gain; }
            set
            {
                double v;
                if (StringUtils.DecodeString(value, out v))
                {
                    _gain = value;
                    gain = v;
                    //wasparsed = RaisePropertyChanged(v, ref gain);
                }
            }
        }

        public VoltageControlledGenerator(ComponentContainer owner, string name)
            : base(owner)
        {
            InputNodes = new List<NodeSingle>();
            Initialize(name);
            //Gain = "10";
        }

        //private void CheckParsing()
        //{
        //    if (!wasparsed)
        //    {
        //        gain = StringUtils.DecodeString(Gain);
        //        wasparsed = true;
        //    }
        //}

        public override Complex voltage(NodeSingle referenceNode, Complex? W = null)
        {
            Complex v = InputNodes[0].Voltage;
            if (InputNodes.Count > 1)
            {
                //si no esta una entrada a tierra hay que hacer la diferencia
                v -= InputNodes[1].Voltage;
            }
            v = v * new Complex(gain, 0);
            if (referenceNode == Nodes[0])
                return v;
            else if (referenceNode == Nodes[1])
                return -v;
            else
                return double.NaN;
        }

        public override double voltage(NodeSingle referenceNode, double t)
        {
           // CheckParsing();

            double v = InputNodes[0].Voltage.Real;
            if (InputNodes.Count > 1)
            {
                //si no esta una entrada a tierra hay que hacer la diferencia
                v -= InputNodes[1].Voltage.Real;
            }
            v = v * gain;
            if (referenceNode == Nodes[0])
                return v;
            else if (referenceNode == Nodes[1])
                return -v;
            else
                return double.NaN;
            //return base.voltage(referenceNode, t);
        }

        public override void Reset()
        {
            //wasparsed = false;
            base.Reset();
        }

        public virtual Complex ControllerEquationValue(object node, object e, bool isinput = false)
        {
            if (node is NodeSingle)
            { 
                NodeSingle nodo1 = node as NodeSingle;
                if (Nodes.Contains(nodo1) && !isinput)
                {
                    if (Nodes[0] == nodo1)
                        return 1;
                    else
                        return -1;
                }
                else if (InputNodes.Contains(nodo1) && isinput)
                {
                    if (InputNodes[0] == nodo1)
                        return -gain;
                    else
                        return gain;
                }
                else
                    throw new NotImplementedException();
            }
            else if (node is int && (int)node == 0)
            {
                return Complex.Zero;
            }
            throw new NotImplementedException();
        }
    }
}
