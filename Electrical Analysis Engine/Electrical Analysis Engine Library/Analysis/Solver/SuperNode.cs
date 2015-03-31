using ElectricalAnalysis.Components;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{

    public class SuperNode : List<Node>
    {
        //List<Node> analizedNodes;
        /// <summary>
        /// Identifies the one of the main nodes of the supernode
        /// </summary>
        public Node MainNode { get; set; }

        /// <summary>
        /// If a supernode is detected, find all the nodes in its
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="compo"></param>
        /// <param name="super"></param>
        public static void FindSuperNodeElements(Node nodo, Dipole compo,
                                                SuperNode super, List<Node> nodosanalizados)
        {
            Node nodo1 = compo.OtherNode(nodo);
            if (super.Contains(nodo1))
                return;

            super.Add(nodo1);
            nodo1.TypeOfNode = Node.NodeType.VoltageLinkedNode;
            nodosanalizados.Add(nodo1);

            foreach (var comp1 in nodo.Components)
            {
                if (comp1 == compo)
                    continue;

                if (comp1 is VoltageGenerator || comp1 is Capacitor)
                    FindSuperNodeElements(nodo1, comp1, super, nodosanalizados);
            }
        }


        public void CalculateLinkedVoltages(SolveInfo solveinfo, Complex32 W)
        {
            Node nodo = MainNode;
            foreach (var compo in nodo.Components)
            {
                Node nodo1 = compo.OtherNode(nodo);
                if (compo is VoltageGenerator || compo is Capacitor)
                    nodo1.Voltage = compo.voltage(nodo1, W) + nodo.Voltage;
                //if (this.Contains(nodo))
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        public void CalculateLinkedVoltages(SolveInfo solveinfo, double t)
        {
            Node nodo = MainNode;
            foreach (var compo in nodo.Components)
            {
                Node nodo1 = compo.OtherNode(nodo);
                if (nodo1.IsReference)
                    continue;

                if (compo is VoltageGenerator || compo is Capacitor)
                {
                    nodo1.Voltage = new Complex32(-(float)compo.voltage(nodo1, t), 0) + nodo.Voltage;
                    solveinfo.calculatednodes.Add(nodo1);
                }
                //if (this.Contains(nodo))
            }

        }

        public virtual double GetTotalImpedance(double t)
        {
            //analizedNodes = new List<Node>();
            double z = 0, y = 0;
            foreach (var nodo in this)
            {
                foreach (var comp in nodo.Components)
                {
                    if (comp is Resistor)
                    {
                        if (Contains(comp.Nodes[0]) && Contains(comp.Nodes[1]))
                        {
                            //las resistencias entre nodos del supernodo son ignoradas
                            continue;
                        }
                        z = comp.Impedance().Real;
                        y += 1 / z;
                    }
                }
            }
            return y;
        }


    }
}
