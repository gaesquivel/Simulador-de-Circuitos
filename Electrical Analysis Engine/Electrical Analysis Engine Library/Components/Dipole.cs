using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public abstract class Dipole: Item
    {

        protected Complex32 _current;//, voltage;



        [Browsable(false)]
        public ComponentContainer Owner { get; set; }

        [Browsable(false)]
        public Circuit OwnerCircuit { get; set; }

        [Browsable(false)]
        public bool IsConnectedToEarth
        {
            get
            {
                foreach (var item in Nodes)
                    if (item.IsReference)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Corriente incremental, dependiente del tiempo en algunos componentes
        /// </summary>
        /// <param name="referenceNode"></param>
        /// <param name="CurrentTime"></param>
        /// <returns></returns>
        public virtual double Current(Node referenceNode, double CurrentTime)
        {
            if (referenceNode == Nodes[0])
                return _current.Real;
            else
                return -_current.Real;
        }

        public virtual double voltage(Node referenceNode, double CurrentTime)
        {
            if (referenceNode == Nodes[0])
                return Voltage.Real;
            return -Voltage.Real;
        }

        public virtual Complex32 Current(Node referenceNode, Complex32? W = null)
        {
            //el signo contrario al pensado, entra por neagtivo y sale por positivo
            if (referenceNode == Nodes[0])
                return -current;
            else
                return current;
        }

        /// <summary>
        /// Valor de la corriente de continua
        /// </summary>
        [Browsable(false)]
        public virtual Complex32 current
        {
            get
            {
                return _current;
            }
            internal set
            {
                _current = value;
            }
        }


      


        public virtual Complex32 voltage(Node ReferenceNode, Complex32 ?W = null)
        {
            return 0;
            //if (ReferenceNode == Nodes[0])
            //    return Voltage;
            //if (ReferenceNode == Nodes[1])
            //    return -Voltage;
            //return Complex32.NaN;
        }

        /// <summary>
        /// DC operating voltage
        /// </summary>
        [Browsable(false)]
        public virtual Complex32 Voltage
        {
            get { return Nodes[0].Voltage - Nodes[1].Voltage; }
            //set { voltage = value; }
        }

        /// <summary>
        /// Listado de nodos de un componente. son 2 nada mas
        /// </summary>
        [Browsable(false)]
        public List<Node> Nodes { get; protected set; }


        [Browsable(false)]
        public int ReferenceNode { get; set; }

        public Dipole(ComponentContainer owner)
            : base()
        {
            Nodes = new List<Node>();
            Nodes.Add(new Node());
            Nodes.Add(new Node());
            Owner = owner;
            if (owner is Circuit)
                OwnerCircuit = (Circuit)owner;
        }

         [DebuggerStepThrough]
        public Node OtherNode(Node thisnode)
        {
            if (thisnode == Nodes[0])
                return Nodes[1];
            return Nodes[0];
        }

        public virtual Complex32 Impedance(Complex32? W = null)
        {
            return Complex32.Zero;
        }

        public virtual void Reset()
        {
            //Voltage = Complex32.Zero;
            _current = Complex32.Zero;
        }

        public override string ToString()
        {
            return "Component " + Name + ", Current " + current.ToString();
        }

    }
}
