using System;
using ElectricalAnalysis;
using ElectricalAnalysis.Components;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Analysis;
using System.Drawing;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace ElectricalAnalysis_Test
{
    class Program
    {
        static Circuit cir = new Circuit();
        //static Circuit cir2 ;
        static ComplexPlainSolver sol1;

        static void Main(string[] args)
        {
            int i = 4;
           
            switch (i)
            {
                case 0:
                    #region RLCharge
                    cir.ReadCircuit("circuits/RLcharge.net");
                   // cir = (Circuit)cir.Clone();
                    //DCSolver.Optimize(cir2);
                    DCAnalysis ac0 = (DCAnalysis)cir.Setup[0];
                    DCSolver solver = (DCSolver)ac0.Solver;
                    //solver.Solve(cir2, );
                    cir.Solve();
                    solver.Export("e:/Test.csv");
                    #endregion
                    break;
                case 1:
                    #region testidc
                    //cir.ReadCircuit("circuits/testidc.net");
                    //cir.ReadCircuit("circuits/testdc3.6.net");  //ok
                    //cir.ReadCircuit("circuits/testdc3.6.RCLeq.net");  //ok
                    //cir.ReadCircuit("circuits/DC Bias/RIV.net");  //ok
                    cir.ReadCircuit("circuits/DC Bias/RVI.net");  //ok
                    DCAnalysis ac3 = (DCAnalysis)cir.Setup[0];
                    DCSolver solver3 = (DCSolver)ac3.Solver;
                    cir.Solve();
                    solver3.Export("e:/Test.csv");
                    #endregion
                    break;
                case 2:
                    #region derivador
                    cir.ReadCircuit("circuits/derivador.net");
                    //cir.ReadCircuit("RCL.net");
                    //cir2 = (Circuit)cir.Clone();
                    cir.Setup.RemoveAt(0);
                    ACAnalysis ac = new ACAnalysis();
                    cir.Setup.Add(ac);
                    //ACSweepSolver.Optimize(cir2);
                    cir.Solve();

                    ACSweepSolver sol = (ACSweepSolver)ac.Solver;
                    sol.Export("ACSweep.csv");
                    #endregion
                    break;
                case 3:
                    #region RLC plano complejo
                    cir.ReadCircuit("circuits/RLC.net");
                    cir.Setup.RemoveAt(0);
                    ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
                    cir.Setup.Add(ac1);
                    cir.Solve();
                    sol1 = (ComplexPlainSolver)ac1.Solver;
                    sol1.Export("e:/plain.csv");
                    Bitmap bmp =  FileUtils.DrawImage(func, ac1.Points, ac1.Points);
                    bmp.Save("e:/plain.bmp");
                    #endregion
                    break;
                case 4:
                    #region RL, RC, RCL transitorio
                    //cir.ReadCircuit("circuits/RCL.net");
                    cir.ReadCircuit("circuits/RLC.net");
                    //cir.ReadCircuit("circuits/RLcharge.net");
                    cir.Setup.RemoveAt(0);
                    TransientAnalysis ac5 = new TransientAnalysis();
                    ac5.Step = "10n";
                    ac5.FinalTime = "10u";
                    cir.Setup.Add(ac5);
                    TransientSolver sol5 = (TransientSolver)ac5.Solver;
                    cir.Solve();
                    sol5.Export("e:/time.csv");
                    #endregion
                    break;
                case 5:
                    #region vsingain
                    cir.ReadCircuit("circuits/vsingain.net");
                    //cir.ReadCircuit("RCcharge.net");
                    //c//ir = (Circuit)cir.Clone();
                    cir.Setup.RemoveAt(0);
                    TransientAnalysis ac6 = new TransientAnalysis();
                    ac6.Step = "100n";
                    cir.Setup.Add(ac6);
                    TransientSolver sol6 = (TransientSolver)ac6.Solver;
                    //sol6.Optimize(cir2);
                    cir.Solve();
                    sol6.Export("e:/time.csv");
                    #endregion
                    break;
                case 6:
                    #region rectangular matrix
                    int N = 3, M = N + 1;
                    var v = Vector<double>.Build.Random(M);
                    var A = Matrix<double>.Build.Random(M, N);

                    var x = A.Solve(v);
                    Console.WriteLine(v.ToString());
                    Console.WriteLine(A.ToString());
                    Console.WriteLine(x.ToString());
                    #endregion
                    break;
                case 7:
                    #region Test dc new algorithm
                    cir.ReadCircuit("circuits/testdc3.6.net");
                    //cir2 = (Circuit)cir.Clone();
                    DCAnalysis ac7 = (DCAnalysis)cir.Setup[0];
                    DCSolver solver7 = (DCSolver)ac7.Solver;
                    cir.Solve();
                    solver7.Export("e:/Test.csv");


                    #endregion
                    break;
                case 8:
                    #region gain
                    cir.ReadCircuit("circuits/vsingain2.net");
                    DCAnalysis ac8 = (DCAnalysis)cir.Setup[0];
                    DCSolver solver8 = (DCSolver)ac8.Solver;
                    cir.Solve();
                    solver8.Export("e:/Test.csv");
                    #endregion
                    break;

                case 9:
                    #region 3 etapas
                    cir.ReadCircuit("circuits/Amplificador3Et.net");
                    DCAnalysis ac9 = (DCAnalysis)cir.Setup[0];
                    DCSolver solver9 = (DCSolver)ac9.Solver;
                    cir.Solve();
                    solver9.Export("e:/Test.csv");
                    #endregion
                    break;
                case 10:
                    #region Laplace
                    cir.ReadCircuit("circuits//AmpLaplace.net");
                    DCAnalysis ac10 = (DCAnalysis)cir.Setup[0];
                    DCSolver solver10 = (DCSolver)ac10.Solver;
                    cir.Solve();
                    solver10.Export("e:/Test.csv");
                    #endregion
                    break;
                case 11:
                    #region RLC transient
                   // cir.ReadCircuit("circuits//RCL.net");   //OK
                    cir.ReadCircuit("circuits//RCL.net");   //OK
                    cir.Setup.RemoveAt(0);

                    TransientAnalysis ac11 = new TransientAnalysis()
                                        { Step = "10n", FinalTime = "10u"};
                    //ac11.Step = "10n";
                    cir.Setup.Add(ac11);
                    TransientSolver solver11 = (TransientSolver)ac11.Solver;
                    cir.Solve();
                    solver11.Export("e:/Test.csv");
                    #endregion
                    break;
                default:
                    break;
            }

            Console.ReadKey();  
      }


        static Complex func(int x, int y)
        {
            Complex W = sol1.WfromIndexes[new Tuple<int, int>(x, y)];
            foreach (var node in  sol1.Voltages[W])
            {
                if (node.Key == "out")
                {
                    return node.Value;
                }
            }
            return Complex.Zero;
        }
   }
}
