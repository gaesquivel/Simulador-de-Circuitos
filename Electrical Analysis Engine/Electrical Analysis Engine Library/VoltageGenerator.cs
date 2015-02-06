using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class VoltageGenerator:Generator
    {

        public VoltageGenerator()
            : base()
        {
            Initialize("V" + ID.ToString());
            Expresion = "V";
            Value = 1000;
        }

        public VoltageGenerator(string name, string value):base()
        {
            Initialize(name, value);
        }



    }
}
