using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Data
{
    public class ACSweepData:SimulationData
    {
        /// <summary>
        /// W, junto a una matriz con los valores de las tensiones de los nodos
        /// </summary>
        public Dictionary<double, Matrix<Complex>> Results { get; protected set; }

        public ACSweepData()
            : base()
        {
            Results = new Dictionary<double, Matrix<Complex>>();
        }

    }
}
