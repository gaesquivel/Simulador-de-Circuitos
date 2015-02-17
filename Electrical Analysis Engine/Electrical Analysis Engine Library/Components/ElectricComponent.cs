using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public abstract class ElectricComponent: Dipole
    {


        //Expresion R, L, C
        public string Expresion { get; set; }   //model?
        public double Value { get; set; }   //1000 (ohms, Henry...)
        public double Temperature { get; set; }


     

        public ElectricComponent()
            : base()
        { 
        }

        protected void Initialize(string name = null, string value = null)
        { 
            if (string.IsNullOrEmpty(name))
                Name = "Component" + ID.ToString();
            else
                Name = name;

            if (string.IsNullOrEmpty(value))
                Value = 0;
            else
            {
                Value = StringUtils.DecodeString(value);
            }
        }

    }
}
