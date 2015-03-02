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

        public Dipole CurrentImposser { get; set; }

        public Branch(ComponentContainer owner):base(owner)
        {
            Name = "br" + ID.ToString();
            InternalNodes = new List<Node>();
        
        }

        public virtual Complex32 TheveninVoltage(Node referenceNode, Complex32 ?W = null)
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
                if (compo1 is Branch)
                    v += ((Branch)compo1).TheveninVoltage(node1, W);                    
                else
                    v += compo1.voltage(node1, W);
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v;
        }

        public virtual double TheveninVoltage(Node referenceNode, double t)
        {
            double v = 0;
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
                if (compo1 is Branch)
                    v += ((Branch)compo1).TheveninVoltage(node1, t);
                else
                    v += compo1.voltage(node1, t);
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v;
        }


        public virtual double NortonCurrent(Node referenceNode, double t)
        {
            if (CurrentImposser != null)
            {
                return Current(referenceNode, t);
            }

            double v = 0, z = 0;
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
                if (compo1 is Branch)
                    v += ((Branch)compo1).TheveninVoltage(node1, t);
                else
                    v += compo1.voltage(node1, t);
                z += compo1.Impedance().Real;
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v / z;
        }


        public virtual Complex32 NortonCurrent(Node referenceNode, Complex32 ?W = null)
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
                if (compo1 is Branch)
                    v += ((Branch)compo1).TheveninVoltage(node1, W);
                else
                    v += compo1.voltage(node1, W);
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
                Complex32 voltage = Complex32.Zero;
                foreach (var item in Components)
                {
                    voltage += item.Voltage;
                }
                return voltage;
            }
            //set
            //{
            //    base.Voltage = value;
            //}
        }

        //la corriente puede variar en el tiempo
        public override double Current(Node referenceNode, double t)
        {
            double i = 0;
            if (CurrentImposser != null)
            {
                if (CurrentImposser.Nodes.Contains(referenceNode))
                {
                    i = CurrentImposser.Current(referenceNode, t);
                    _current = new Complex32((float)i, 0);
                }
                else
                {
                    //hay que buscar nodo a nodo 
                    throw new NotImplementedException();
                }
            }
            else
            {
                foreach (var item in Components)
                {
                    //los generadores de corriente imponen la corriente en una rama!
                    if (item is CurrentGenerator)
                    {
                        CurrentImposser = item;
                        i = item.Current(referenceNode, t);
                        break;
                    }
                    //si no hay generadores de corriente la corriente la imponen lo inductores
                    else if (item is Inductor)
                    {
                        CurrentImposser = item;
                        i = item.Current(referenceNode, t);
                    }
                }
            }

            if (CurrentImposser == null)
                i = _current.Real;

            if (referenceNode == Nodes[0])
                return -i;
            else
                return i;
        } 

        public override Complex32 current
        {
            get
            {
                if (CurrentImposser != null)
                {
                    return CurrentImposser.current;
                }
                //los generadores de corriente imponen la corriente en una rama!
                foreach (var item in Components)
                {
                    if (item is CurrentGenerator)
                    {
                        CurrentImposser = item;
                        return item.current;
                    }
                }
                return _current;
            }
            internal set
            {
                _current = value;
            }
        }


        public List<Node> InternalNodes { get; protected set; }

     

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
