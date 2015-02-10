using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Components
{
    public class Resistor:PasiveComponent
    {
       
        public Resistor()
            : base()
        {
            Initialize("R" + ID.ToString());
            //Name = "R" + ID.ToString();
            Expresion = "R";
            Value = 1000;
        }

        public Resistor(string name, string value):base()
        {
            Initialize(name, value);

        }

    }
}
