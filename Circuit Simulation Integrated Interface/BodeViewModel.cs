using ElectricalAnalysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using ElectricalAnalysis.Analysis.

namespace Simcircuit
{
    public class BodeViewModel
    {

        public ObservableCollection<Point> Collection { get; set; }
        public BodeViewModel()
        {
            Collection = new ObservableCollection<Point>();
            this.Collection.Add(new Point(0, 1));
            this.Collection.Add(new Point(1, 2));
            this.Collection.Add(new Point(2, 3));
            this.Collection.Add(new Point(3, 4));
        }

        public void GenerateDatas()
        {
            try
            {
                Collection.Clear();
                Circuit cir = new Circuit();
                cir.ReadCircuit("Circuits/RCcharge.net");
                Circuit cir2 = (Circuit)cir.Clone();
                cir2.Setup.RemoveAt(0);
                ACAnalysis ac = new ACAnalysis();
                cir2.Setup.Add(ac);
                ACSweepSolver.Optimize(cir2);
                cir2.Solve();

                ACSweepSolver sol = (ACSweepSolver)ac.Solver;

                foreach (var res in sol.Results)
                {
                    //Console.Write(res.Key.ToString() + "rad/seg");
                    foreach (var nodo in res.Value)
                    {
                        if (nodo.Key == "$N_0001")
                            //Console.Write(nodo.Key + " " + nodo.Value.ToString() + "V\r\n");
                            this.Collection.Add(new Point(nodo.Value.Magnitude, res.Key));
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
        
        }
    }
}
