using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis
{
    public class Capacitor:PasiveComponent
    {
        public Capacitor(double value = 1E-5)
            : base()
        {
            Name = "C" + ID.ToString();
            Expresion = "C";
            Value = value;
        }

    }
}
