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
using ElectricalAnalysis.Analysis;
//using Matrix = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;

namespace ElectricalAnalysis_Test
{
   class Program
   {
        static void Main(string[] args)
        {
            int i = 3;
            Circuit cir = new Circuit();
            Circuit cir2 ;

            switch (i)
            {
                case 0:

            //cir.ReadCircuit("testdc.net");
            //DCSolver solver = new DCSolver();
            //DCSolver.Optimize(cir2);

                    break;
                case 1:
                    cir.ReadCircuit("RCcharge.net");
                    cir2 = (Circuit)cir.Clone();
                    cir2.Setup.RemoveAt(0);
                    ACAnalysis ac = new ACAnalysis();
                    cir2.Setup.Add(ac);
                    ACSweepSolver.Optimize(cir2);
                    cir2.Solve();

                    ACSweepSolver sol = (ACSweepSolver)ac.Solver;

                    break;

                case 3:
                    cir.ReadCircuit("RCcharge.net");
                    cir2 = (Circuit)cir.Clone();
                    cir2.Setup.RemoveAt(0);
                    ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
                    cir2.Setup.Add(ac1);
                    ACSweepSolver.Optimize(cir2);
                    cir2.Solve();
                    ComplexPlainSolver sol1 = (ComplexPlainSolver)ac1.Solver;


                    break;


                default:
                    break;
            }

      
       


            Console.ReadKey();
      }
   }
}
