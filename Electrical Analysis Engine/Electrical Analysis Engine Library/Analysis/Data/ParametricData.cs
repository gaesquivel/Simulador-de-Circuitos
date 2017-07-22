using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Data
{
    /// <summary>
    /// Allow to storage some pairs of data: Parameter Value, asociated Data 
    /// </summary>
    public class ParametricData:Dictionary<Complex, DataBase>, DataBase
    {


    }
}
