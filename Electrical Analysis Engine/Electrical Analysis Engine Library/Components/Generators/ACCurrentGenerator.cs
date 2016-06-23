using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components.Generators
{
    public class ACCurrentGenerator: CurrentGenerator
    {
        public virtual Complex ACCurrent { get; set; }

        public ACCurrentGenerator(ComponentContainer owner):base(owner)
        {
            ACCurrent = new Complex(1, 0);


        }

        public ACCurrentGenerator(ComponentContainer owner, float ACMagnitude = 1, float ACPhase = 0)
            : base(owner)
        {
            ACCurrent = Complex.FromPolarCoordinates(ACMagnitude, ACPhase);
        }

        public override Complex Current(NodeSingle referenceNode, Complex? W = default(Complex?))
        {
            return ACCurrent;
        }
    }
}
