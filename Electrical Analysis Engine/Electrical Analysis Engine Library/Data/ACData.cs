using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Data
{
    public class ACSweepData:SimulationData
    {
        /// <summary>
        /// W, junto a una matriz con los valores de las tensiones de los nodos
        /// </summary>
        public Dictionary<double, Matrix<Complex32>> Results { get; protected set; }

        public ACSweepData()
            : base()
        {
            Results = new Dictionary<double, Matrix<Complex32>>();
        }

    }
}
