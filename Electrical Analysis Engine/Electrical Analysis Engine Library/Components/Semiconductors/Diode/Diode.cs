using ElectricalAnalysis.Components.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components.Semiconductors.Diode
{
    public class Diode : Model
    {
 
        [DefaultValue("1E-14")]
        [Description("Saturation current (diode equation)")]
        public string IS { get; set; }

        [DefaultValue("400")]
        [Description("Reverse breakdown voltage")]
        public string BV { get; set; }

        [DefaultValue("1m")]
        [Description("Reverse breakdown current")]
        public string IBV { get; set; }

        [DefaultValue("0")]
        [Description("Parsitic resistance (series resistance)")]
        public string RS { get; set; }

        [DefaultValue("1n")]
        [Description("")]
        public string GMIN { get; set; }

        [DefaultValue("1")]
        [Description("Emission coefficient, 1 to 2")]
        public string N { get; set; }


        public Diode(ComponentContainer owner) : base(owner)
        {

            SubCircuit comun = new SubCircuit(owner);
            comun.Components.Add(new Resistor(this, "RS", RS));
            comun.Components.Add(new Resistor(this, "GMIN", GMIN));
            comun.Components.Add(new CurrentGenerator(this));
        }

        //public override double Current(NodeSingle referenceNode, double CurrentTime)
        //{
        //    return base.Current(referenceNode, CurrentTime);
        //}


    }
}
