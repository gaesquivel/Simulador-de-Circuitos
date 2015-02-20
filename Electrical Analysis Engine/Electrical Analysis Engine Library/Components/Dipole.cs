using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public abstract class Dipole: Item
    {

        protected Complex32 current, voltage;
        public ComponentContainer Owner { get; set; }

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

        public virtual Complex32 NortonCurrent(Node referenceNode, Complex32 ?W = null) {
            return 0;
        }
        public virtual Complex32 TheveninVoltage(Node referenceNode, Complex32? W = null)
        {
            return 0;
        }


        /// <summary>
        /// Valor de la corriente de continua
        /// </summary>
        public virtual Complex32 Current
        {
            get
            {
                if (this is Capacitor)
                    return 0;

                if (this is Inductor)
                    throw new NotImplementedException();
                //if (this is VoltageGenerator)
                //    throw new NotImplementedException();
                
                return Complex32.Zero;
            }
            internal set
            {
                current = value;
            }
        }


        public virtual Complex32 Voltage
        {
            get { return voltage; }
            set { voltage = value; }
        }

        /// <summary>
        /// Listado de nodos de un componente. son 2 nada mas
        /// </summary>
        public List<Node> Nodes { get; protected set; }
        public int ReferenceNode { get; set; }

        public Dipole(ComponentContainer owner)
            : base()
        {
            Nodes = new List<Node>();
            Nodes.Add(new Node());
            Nodes.Add(new Node());
            Owner = owner;
        }
        
        public Node OtherNode(Node thisnode)
        {
            if (thisnode == Nodes[0])
                return Nodes[1];
            return Nodes[0];
        }


        //public virtual Complex32 Impedance(double W = 0)
        //{
        //    return Complex32.Zero;
        //}

        public virtual Complex32 Impedance(Complex32? W = null)
        {
            return Complex32.Zero;
        }

        public virtual void Reset()
        {
            Voltage = Complex32.Zero;
            Current = Complex32.Zero;
        }

    }
}
