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
            cir.ReadCircuit("testdc.net");
            DCSolver solver = new DCSolver();
            Circuit cir2 = (Circuit)cir.Clone();
            DCSolver.Optimize(cir2);
            cir2.Solve();

            foreach (var item in cir2.Nodes)
            {
                Console.Write(item.Key + " " + item.Value.Voltage.ToString() + "V\r\n");
            }

            Console.Write(cir2.StaticVector.ToString());
            Console.Write(cir2.StaticMatrix.ToString());
            //Console.Write(cir2.Nodes.Keys.ToString());
            Console.Write(cir2.StaticResult.ToString());


            Console.ReadKey();
      }
   }
}
