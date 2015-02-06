using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    abstract class StringUtils
    {

        
        /// <summary>
        /// converts strings representing numbers expressed 
        /// in Engieniring formar, just like:
        /// 1K2
        /// 12.2Meg
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double DecodeString(string measure)
        {
            var re = new Regex(@"(?i)(?<value>-?[\d.]+)\s*(?<multiplier>[kgtmunp]?)\s*(?<unit>[avw]*)");
            var conversionFactors = new Dictionary<string, double> {
                                { "", 1 }, 
                                { "k", 1E3 }, 
                                { "meg", 1E6 }, 
                                { "g", 1E9 }, 
                                { "t", 1E12 }, 
                                { "m", 1E-3 }, 
                                { "u", 1E-6 }, 
                                { "n", 1E-9 }, 
                                { "p", 1E-12 } };

            var m = re.Match(measure);

            double value = Convert.ToDouble(m.Groups["value"].Value);
            return value * conversionFactors[m.Groups["multiplier"].Value.ToLower()];

        }
    }
}
