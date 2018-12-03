using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace ElectricalAnalysis.Data
{
    public class TransientData:SimulationData
    {
        /// <summary>
        /// t, junto a una matriz con los valores de las tensiones de los nodos
        /// </summary>
        public Dictionary<double, Matrix<double>> Results { get; protected set; }

        public TransientData()
            : base()
        {
            Results = new Dictionary<double, Matrix<double>>();
        }

    }
}
