using System;
using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    /// <summary>
    /// This is a block of serial conected components
    /// </summary>
    public class Branch: Block
    {
        double previoustime;

        public Dipole CurrentImposser { get; set; }

        public Branch(ComponentContainer owner):base(owner)
        {
            Name = "br" + ID.ToString();
            InternalNodes = new List<Node>();
        
        }

        public virtual Complex TheveninVoltage(NodeSingle referenceNode, Complex ?W = null)
        {
            Complex v = 0;
            Dipole compo1 = null;
            NodeSingle node1 = null;
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

        public virtual double TheveninVoltage(NodeSingle referenceNode, double t)
        {
            double v = 0;
            Dipole compo1 = null;
            NodeSingle node1 = null;
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


        public virtual double NortonCurrent(NodeSingle referenceNode, double t)
        {
            if (CurrentImposser != null)
            {
                return Current(referenceNode, t);
            }

            double v = 0, z = 0;
            Dipole compo1 = null;
            NodeSingle node1 = null;
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
                else if(compo1 is VoltageGenerator || compo1 is Capacitor)
                    v += compo1.voltage(node1, t);
                else if (compo1 is Resistor)
                    z += compo1.Impedance().Real;
                //v += compo1.voltage(node1, t);
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v / z;
        }


        public virtual Complex NortonCurrent(NodeSingle referenceNode, Complex ?W = null)
        {
            Complex v = 0, z = 0;
            Dipole compo1 = null;
            NodeSingle node1 = null;
            //encuentro el componente unido al nodo de referencia
            foreach (var item in Components)
            {
                if (item.Nodes[0] == referenceNode || item.Nodes[1] == referenceNode)
                {
                    compo1 = item;
                    break;
                }
            }

            //a partir del nodo y el componente, escaneo la ramaenbusca de la Vthevenin y la Rthevenin
            node1 = referenceNode;
            do
            {
                if (compo1 is Branch)
                    v += ((Branch)compo1).TheveninVoltage(node1, W);
                else if(compo1 is ACVoltageGenerator)
                    v += compo1.voltage(node1, W);

                //los componentes pasivos no tienen tension en barrido en frecuencia
                z += compo1.Impedance(W);
                node1 = compo1.OtherNode(node1);
                compo1 = node1.OtherComponent(compo1);
            } while (InternalNodes.Contains(node1));

            return v / z;
        }

        public override Complex Voltage
        {
            get
            {
                Complex voltage = Complex.Zero;
                foreach (var item in Components)
                {
                    voltage += item.Voltage;
                }
                return voltage;
            }
        }

        //la corriente puede variar en el tiempo
        public override double Current(NodeSingle referenceNode, double t)
        {
            //si ya se calculo la corriente devuelvo la calculada
            if (previoustime > 0 && previoustime == t)
            {
                if (referenceNode == Nodes[0])
                    return _current.Real;
                else if (referenceNode == Nodes[1])
                    return -_current.Real;
                else
                    throw new NotImplementedException();
            }

            double i = 0;
            if (CurrentImposser != null)
            {
                if (CurrentImposser.Nodes.Contains(referenceNode))
                {
                    i = CurrentImposser.Current(referenceNode, t);
                    if (referenceNode  == Nodes[0])
                        _current = new Complex(i, 0);
                    else
                        _current = new Complex(-i, 0);
                }
                else
                {
                    //hay que buscar nodo a nodo 
                    NodeSingle nodo = FindComponentNode(referenceNode, CurrentImposser);
                    i = CurrentImposser.Current(nodo, t);
                }
            }
            else
            {
                foreach (var comp in Components)
                {
                    //los generadores de corriente imponen la corriente en una rama!
                    if (comp is CurrentGenerator)
                    {
                        CurrentImposser = comp;
                        i = comp.Current(referenceNode, t);
                        break;
                    }
                    //si no hay generadores de corriente la corriente la imponen lo inductores
                    else if (comp is Inductor)
                    {
                        CurrentImposser = comp;
                        i = comp.Current(referenceNode, t);
                    }
                    else if (comp is Resistor)
                    {
                        CurrentImposser = comp;
                        i = comp.Current(referenceNode, t);
                    }
                }
            }

            if (CurrentImposser == null)
                i = _current.Real;

            if (referenceNode == Nodes[0])
                return -i;
            else// if (referenceNode == Nodes[1])
                return i;
            //else
                throw new NotImplementedException();
        } 

        public override Complex current
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

     

        public override Complex Impedance(Complex ?W = null)
        {
            Complex Z = Complex.Zero;
            foreach (var item in Components)
            {
                Z += item.Impedance(W);
            }

            return Z;
        }

        public override void Reset()
        {
            previoustime = 0;
            base.Reset();
        }


        /// <summary>
        /// Dado 1 nodo extremo de la rama, busca el nodo mas cercano al componente
        /// </summary>
        /// <param name="externalnode">one of two exterior branch nodes</param>
        /// <returns></returns>
        public NodeSingle FindComponentNode(NodeSingle externalnode, Dipole component)
        {
            NodeSingle n = null;
            if (!Nodes.Contains(externalnode))
            {
                //la rama no contiene al nodo indicado, probablemene un error
                return null;
            }

            Dipole comp = null;
            //if (Nodes.Contains(originalnode))

            //identifico al componente que contiene el nodo externo
            foreach (var comp1 in Components)
            {
                if (comp1.Nodes[0] == externalnode || comp1.Nodes[1] == externalnode)
                {
                    comp = comp1;
                    break;
                }
            }

            n = externalnode;
            do
            {
                if (comp == component)
                {
                    return n;
                }
                //busco componente a componente, nodo a nodo
                n = comp.OtherNode(n);
                comp = n.OtherComponent(comp);

            } while (InternalNodes.Contains(n));

            //error, no se encontro elcompente que contine el nodo!
            return null;
        }


        /// <summary>
        /// Dado 1 nodo interno de la rama, busca el nodo externo mas cercano al componente
        /// </summary>
        /// <param name="internalnode"></param>
        /// <returns></returns>
        public Node FindBranchExternalNode(NodeSingle internalnode, Dipole component)
        {
            if (!InternalNodes.Contains(internalnode))
            {
                //la rama no contiene al nodo indicado, probablemene un error
                return null;
            }
            if (!component.Nodes.Contains(internalnode))
            {
                //el componente no contiene el nodo, un error
                return null;
            }

            Dipole comp = null, comp2 = null;

            NodeSingle n = null;
            n = internalnode;
            throw new NotImplementedException();
            while (!Nodes.Contains(n))
            {
                //busco componente a componente, nodo a nodo
                comp2 = n.OtherComponent(comp);
                n = comp2.OtherNode(n);

            }

            return n;
        }

        public override string ToString()
        {
            string s = base.ToString();
            foreach (var comp in Components)
            {
                s += ", " + comp.Name;
            }
            return s;
        }

    }
}
