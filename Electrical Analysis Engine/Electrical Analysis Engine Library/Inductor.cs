using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis
{
    public class Inductor:PasiveComponent
    {
        public Inductor()
            : base()
        {
            Name = "L" + ID.ToString();
            Expresion = "L";
            Value = 1E-4;
        }

    }
}
