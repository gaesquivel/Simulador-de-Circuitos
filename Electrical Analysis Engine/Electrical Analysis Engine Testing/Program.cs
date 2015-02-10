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
//using Matrix = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;

namespace ElectricalAnalysis_Test
{
   class Program
   {
        static void Main(string[] args)
        {
           
            
            //var A = Matrix<double>.Build.DenseOfArray(new double[,] {
            //    { 0.25, 1, 1 },
            //    { 0, 1, -1 },
            //    { 0, 0, 1 }
            //});
            //var b = Vector<double>.Build.Dense(new double[] { 0, 10, -5 });
            //var x = A.Solve(b);
            //Console.Write(x.ToString());

            Circuit cir = new Circuit();
            cir.ReadCircuit("testidc.net");
            cir.Solve();

            Console.Write(cir.StaticVector.ToString());
            Console.Write(cir.StaticMatrix.ToString());
            //Console.Write(cir.Nodes.Keys.ToString());
            Console.Write(cir.StaticResult.ToString());


            Console.ReadKey();
      }
   }
}
