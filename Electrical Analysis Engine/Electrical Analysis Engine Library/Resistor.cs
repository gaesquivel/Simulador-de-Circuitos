using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis
{
    public class Resistor:PasiveComponent
    {
        private string p1;
        private string p2;


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
