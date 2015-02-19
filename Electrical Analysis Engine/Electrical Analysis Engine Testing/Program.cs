using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis;
using ElectricalAnalysis.Components;
using MathNet.Numerics.LinearAlgebra.Complex32;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ElectricalAnalysis.Analysis.Solver;
//using Matrix = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;

namespace ElectricalAnalysis_Test
{
   class Program
   {
        static void Main(string[] args)
        {
           
            Circuit cir = new Circuit();
            //cir.ReadCircuit("testdc.net");
            cir.ReadCircuit("RCcharge.net");
            //DCSolver solver = new DCSolver();
           // ACSweepSolver solver = new ACSweepSolver();
            Circuit cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            ACAnalysis ac = new ACAnalysis();
            cir2.Setup.Add(ac);
            //DCSolver.Optimize(cir2);
            ACSweepSolver.Optimize(cir2);
            cir2.Solve();

            //ACAnalysis ac = (ACAnalysis)cir2.Setup[0];
            ACSweepSolver sol = (ACSweepSolver)ac.Solver;
            foreach (var res in sol.Results)
            {
                Console.Write(res.Key.ToString() + "rad/seg");
                foreach (var nodo in res.Value)
                {
                    if (nodo.Key == "$N_0001")
                        Console.Write(nodo.Key + " " + nodo.Value.ToString() + "V\r\n");
                }
                
            }

            //foreach (var item in cir2.Nodes)
            //{
            //    Console.Write(item.Key + " " + item.Value.Voltage.ToString() + "V\r\n");
            //}

            //Console.Write(cir2.StaticVector.ToString());
            //Console.Write(cir2.StaticMatrix.ToString());
            //Console.Write(cir2.Nodes.Keys.ToString());
            //Console.Write(cir2.StaticResult.ToString());


            Console.ReadKey();
      }
   }
}
