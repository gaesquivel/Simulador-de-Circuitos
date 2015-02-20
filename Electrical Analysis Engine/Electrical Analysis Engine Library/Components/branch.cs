using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    /// <summary>
    /// This is a block of serial conected components
    /// </summary>
    public class Branch: Block
    {
        public override Complex32 TheveninVoltage(Node referenceNode, Complex32 ?W = null)
        {
            Complex32 v = 0;
            Dipole compo1 = null;
            Node node1 = null;
            foreach (var item in Components)
            {
                if (item.Nodes[0] == referenceNode || item.Nodes[1] == referenceNode)
                {
                    compo1 = item;
                    break;
                }
            }
            node1 = referenceNode;
            do
            {
                v += compo1.TheveninVoltage(node1, W);
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v;
        }


        public override Complex32 NortonCurrent(Node referenceNode, Complex32 ?W = null)
        {
            Complex32 v = 0, z = 0;
            Dipole compo1 = null;
            Node node1 = null;
            foreach (var item in Components)
            {
                if (item.Nodes[0] == referenceNode || item.Nodes[1] == referenceNode)
                {
                    compo1 = item;
                    break;
                }
            }
            node1 = referenceNode;
            do
            {
                v += compo1.TheveninVoltage(node1, W);
                z += compo1.Impedance(W);
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v / z;
        }

        public override Complex32 Voltage
        {
            get
            {
                voltage = Complex32.Zero;
                foreach (var item in Components)
                {
                    voltage += item.Voltage;
                }
                return voltage;
            }
            set
            {
                base.Voltage = value;
            }
        }


        public override Complex32 Current
        {
            get
            {
                foreach (var item in Components)
                {
                    if (item is CurrentGenerator)
                    {
                        return item.Current;
                    }
                }

                return base.Current;
            }
            internal set
            {
                base.Current = value;
            }
        }


        public List<Node> InternalNodes { get; protected set; }

        public Branch(ComponentContainer owner):base(owner)
        {
            InternalNodes = new List<Node>();
        }

        //public override Complex32 Impedance(double W = 0)
        //{
        //    Complex32 Z = Complex32.Zero;
        //    foreach (var item in Components)
        //    {
        //        Z += item.Impedance(W);
        //    }

        //    return Z;
        //}


        public override Complex32 Impedance(Complex32 ?W = null)
        {
            Complex32 Z = Complex32.Zero;
            foreach (var item in Components)
            {
                Z += item.Impedance(W);
            }

            return Z;
        }


    }
}
