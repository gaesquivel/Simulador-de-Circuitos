using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class CurrentGenerator:Generator
    {

        public CurrentGenerator()
            : base()
        {
            Initialize("I" + ID.ToString());
            Expresion = "I";
            Value = 1E-3;
        }

        public CurrentGenerator(string name, string value)
            : base()
        {
            Initialize(name, value);
        }
    }
}
