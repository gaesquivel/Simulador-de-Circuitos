using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class MathUtil
    {

        /// <summary>
        /// Scale some value x into range (min, max) to fit linearly to the range (0, 100)
        /// or some other custom output range
        /// </summary>
        /// <param name="InputMin"></param>
        /// <param name="InputMax"></param>
        /// <param name="InputValue"></param>
        /// <param name="OutputMax"></param>
        /// <param name="OutputMin"></param>
        /// <returns></returns>
        public static double Scale(double InputMin, double InputMax, double InputValue 
                                    , double OutputMax = 100, double OutputMin = 0)
        { 
            //dx = max - min
            //x = max -> normalizedmax
            //x = min -> 0
            // y = normalizedmax * (x - min)/(max-min)


            return (OutputMax - OutputMin) * (InputValue - InputMin) / (InputMax - InputMin) + OutputMin;
        }


    }
}
