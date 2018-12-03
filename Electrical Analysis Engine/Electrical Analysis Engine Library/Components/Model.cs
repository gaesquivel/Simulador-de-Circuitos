using ElectricalAnalysis.Components.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{

    /// <summary>
    /// It can be a subcircuit, but admit complex mathematical expressions,
    /// and differents subcircuits for diferent types of analisys
    /// </summary>
    public class Model : SubCircuit
    {
        public enum AnalisysType {
            /// <summary>
            /// if no other analisys is specificated, this is used
            /// </summary>
            Default,
            DCOperatingPoint, DCSweept, ACSweept, ACComplexSweept }


        public Dictionary<AnalisysType, SubCircuit> Equivalent { get; protected set; }


        public Model(ComponentContainer owner) : base(owner)
        {
            Equivalent = new Dictionary<AnalisysType, SubCircuit>();
        }
    }
}
