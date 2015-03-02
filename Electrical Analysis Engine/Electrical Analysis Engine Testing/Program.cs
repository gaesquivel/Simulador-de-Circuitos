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
using System.Drawing;
//using Matrix = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;

namespace ElectricalAnalysis_Test
{
   class Program
   {
        static Circuit cir = new Circuit();
        static Circuit cir2 ;
        static ComplexPlainSolver sol1;

        static void Main(string[] args)
        {
            int i = 5;

            switch (i)
            {
                case 0:
                    cir.ReadCircuit("RLcharge.net");
                    cir2 = (Circuit)cir.Clone();
                    DCSolver.Optimize(cir2);
                    DCAnalysis ac0 = (DCAnalysis)cir2.Setup[0];
                    DCSolver solver = (DCSolver)ac0.Solver;
                    //solver.Solve(cir2, );
                    cir2.Solve();
                    solver.ExportToCSV("e:/Test.csv");

                    break;
                case 1:
                    cir.ReadCircuit("testidc.net");
                    cir2 = (Circuit)cir.Clone();
                    DCSolver.Optimize(cir2);
                    DCAnalysis ac3 = (DCAnalysis)cir2.Setup[0];
                    DCSolver solver3 = (DCSolver)ac3.Solver;
                    //solver.Solve(cir2, );
                    cir2.Solve();
                    solver3.ExportToCSV("e:/Test.csv");

                    break;


                case 2:
                    cir.ReadCircuit("RCL.net");
                    cir2 = (Circuit)cir.Clone();
                    cir2.Setup.RemoveAt(0);
                    ACAnalysis ac = new ACAnalysis();
                    cir2.Setup.Add(ac);
                    ACSweepSolver.Optimize(cir2);
                    cir2.Solve();

                    ACSweepSolver sol = (ACSweepSolver)ac.Solver;

                    break;

                case 3:
                    cir.ReadCircuit("RCL.net");
                    cir2 = (Circuit)cir.Clone();
                    cir2.Setup.RemoveAt(0);
                    ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
                    cir2.Setup.Add(ac1);
                    ACSweepSolver.Optimize(cir2);
                    cir2.Solve();
                    sol1 = (ComplexPlainSolver)ac1.Solver;
                    sol1.SelectedNode = sol1.CurrentCircuit.Nodes["out"];
                   // sol1.
                    sol1.ExportToCSV("e:/plain.csv");
                    Bitmap bmp =  FileUtils.DrawImage(func, ac1.Points, ac1.Points);
                    bmp.Save("e:/plain.bmp");
                    break;


                case 4:
                    cir.ReadCircuit("RCL.net");
                    //cir.ReadCircuit("RCcharge.net");
                    cir2 = (Circuit)cir.Clone();
                    cir2.Setup.RemoveAt(0);
                    TransientAnalysis ac5 = new TransientAnalysis();
                    ac5.Step = "10n";
                    cir2.Setup.Add(ac5);
                    TransientSolver sol5 = (TransientSolver)ac5.Solver;
                    TransientSolver.Optimize(cir2);
                    cir2.Solve();
                    sol5.ExportToCSV("e:/time.csv");
                    break;

                case 5:
                    cir.ReadCircuit("vsingain.net");
                    //cir.ReadCircuit("RCcharge.net");
                    cir2 = (Circuit)cir.Clone();
                    cir2.Setup.RemoveAt(0);
                    TransientAnalysis ac6 = new TransientAnalysis();
                    ac6.Step = "100n";
                    cir2.Setup.Add(ac6);
                    TransientSolver sol6 = (TransientSolver)ac6.Solver;
                    TransientSolver.Optimize(cir2);
                    cir2.Solve();
                    sol6.ExportToCSV("e:/time.csv");
                    break;

                default:
                    break;
            }

      
       


            Console.ReadKey();  
      }


        static Complex32 func(int x, int y)
        {
            Complex32 W = sol1.WfromIndexes[new Tuple<int, int>(x, y)];
            foreach (var node in  sol1.Voltages[W])
            {
                if (node.Key == "out")
                {
                    return node.Value;
                }
            }
            return Complex32.Zero;
        }
   }
}
