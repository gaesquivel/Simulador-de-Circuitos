using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
