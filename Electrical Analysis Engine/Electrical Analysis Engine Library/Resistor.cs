using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis
{
    public class Resistor:PasiveComponent
    {

        public Resistor()
            : base()
        {
            Name = "R" + ID.ToString();
            Expresion = "R";
            Value = 1000;
        }

    }
}
