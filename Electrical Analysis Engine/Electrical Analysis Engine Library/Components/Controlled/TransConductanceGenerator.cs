using System;
using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Components.Controlled
{
    public class TransConductanceGenerator: CurrentGenerator, ControlledCurrentGenerator,
        ControllerOpenCircuit
    {
        double gain;
        bool wasparsed;
        string _gain;

        public List<NodeSingle> InputNodes { get; protected set; }

        protected override string DefaultName { get { return "Gm"; } }


        /// <summary>
        /// DC, AC & Transient constant GM Gain (TransConductance)
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
                    wasparsed = RaisePropertyChanged(v, ref gain);
                }
            }
        }



        public TransConductanceGenerator(ComponentContainer owner):base(owner)
        {
            InputNodes = new List<NodeSingle>();
            Gain = "10m";

        }

        public TransConductanceGenerator(ComponentContainer owner, string name) : base(owner)
        {
            Initialize(name);
            InputNodes = new List<NodeSingle>();
            Gain = "10m";
        }

        public override Complex Current(NodeSingle referenceNode, Complex? W = default(Complex?))
        {
            //CheckParsing();
            Complex v = InputNodes[0].Voltage;
            if (InputNodes.Count > 1)
            {
                //si no esta una entrada a tierra hay que hacer la diferencia
                v -= InputNodes[1].Voltage;
            }
            //Iout = gm * vin
            Current(v * new Complex(gain, 0), referenceNode);
            if (referenceNode == Nodes[0])
                return -current;
            return current;
            //return base.Current(referenceNode, W);
        }

        //private void CheckParsing()
        //{
        //    if (!wasparsed)
        //    {
        //        StringUtils.DecodeString(Gain.Replace(",","."), out gain);
        //        wasparsed = true;
        //    //} 
        //}

        public Complex ControllerEquationValue(object node, object e, bool isinput = true)
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

        public override void Reset()
        {
            wasparsed = false;
            base.Reset();
        }
    }
}
