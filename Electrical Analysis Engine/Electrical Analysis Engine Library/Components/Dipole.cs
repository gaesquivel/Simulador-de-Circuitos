using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    /// <summary>
    /// Represents a component with at least two terminals
    /// </summary>
    public abstract class Dipole: Item
    {

        protected Complex _current;//, voltage;

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
        public virtual double Current(NodeSingle referenceNode, double CurrentTime)
        {
            if (referenceNode == Nodes[0])
                return _current.Real;
            else
                return -_current.Real;
        }


        public virtual Complex Current(NodeSingle referenceNode, Complex? W = null)
        {
            //el signo contrario al pensado, entra por neagtivo y sale por positivo
            if (referenceNode == Nodes[0])
                return -current;
            else
                return current;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="referenceNode">
        /// Reference node where the current flow in.
        /// Null if the first node is assumed
        /// </param>
        public virtual void Current(Complex value, NodeSingle referenceNode = null)
        {
            //el signo contrario al pensado, entra por neagtivo y sale por positivo
            if (referenceNode == null || referenceNode == Nodes[0])
                current = value;
            else
                current = -value;
        }

        /// <summary>
        /// Valor de la corriente de continua medida en el terminal 0
        /// </summary>
        [Browsable(false)]
        public virtual Complex current
        {
            get
            {
                return _current;
            }
            private set
            {
                _current = value;
            }
        }



        /// <summary>
        /// Voltage at the time CurrentTime with this reference node 
        /// </summary>
        /// <param name="referenceNode"></param>
        /// <param name="CurrentTime"></param>
        /// <returns></returns>
        public virtual double voltage(NodeSingle referenceNode, double CurrentTime)
        {
            if (referenceNode == Nodes[0])
                return Voltage.Real;
            return -Voltage.Real;
        }

        /// <summary>
        /// Voltage at angular rate W with this reference node 
        /// </summary>
        /// <param name="ReferenceNode"></param>
        /// <param name="W"></param>
        /// <returns></returns>
        public virtual Complex voltage(NodeSingle ReferenceNode, Complex ?W = null)
        {
            return 0;
        }

        /// <summary>
        /// DC operating voltage
        /// </summary>
        [Browsable(false)]
        public virtual Complex Voltage
        {
            get { return Nodes[0].Voltage - Nodes[1].Voltage; }
        }

        /// <summary>
        /// Listado de nodos de un componente. son 2 nada mas
        /// </summary>
        [Browsable(false)]
        public List<NodeSingle> Nodes { get; protected set; }


        [Browsable(false)]
        public int ReferenceNode { get; set; }

        public Dipole(ComponentContainer owner)
            : base()
        {
            Nodes = new List<NodeSingle>();
            Nodes.Add(new NodeSingle());
            Nodes.Add(new NodeSingle());
            Owner = owner;
            if (owner is Circuit)
                OwnerCircuit = (Circuit)owner;
        }

         [DebuggerStepThrough]
        public NodeSingle OtherNode(NodeSingle thisnode)
        {
            if (thisnode == Nodes[0])
                return Nodes[1];
            return Nodes[0];
        }

        public virtual Complex Impedance(Complex? W = null)
        {
            return Complex.Zero;
        }

        public virtual void Reset()
        {
            //Voltage = Complex.Zero;
            _current = Complex.Zero;
        }

        public override string ToString()
        {
            return "Component " + Name + ", Current " + current.ToString();
        }

    }
}
