using ElectricalAnalysis.Components;
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


        public double GetTotalImpedance(double t)
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
