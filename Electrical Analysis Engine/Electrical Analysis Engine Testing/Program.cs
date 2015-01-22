using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis;

namespace ElectricalAnalysis_Test
{
   class Program
   {
      static void Main(string[] args)
      {
          Resistor r1 = new Resistor();
          //r1.Nodes[0].
          Circuit circuito1 = new Circuit();
          circuito1.Components.Add(r1);



          Console.ReadKey();
      }
   }
}
