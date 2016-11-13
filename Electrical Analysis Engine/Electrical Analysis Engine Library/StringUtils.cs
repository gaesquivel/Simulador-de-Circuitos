using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ElectricalAnalysis
{

    public class EngineeringFormatter : IFormatProvider, ICustomFormatter
    {
        static Dictionary<int, string> multiplies = new Dictionary<int, string>() { 
                                                                                    {12, "T"},
                                                                                    {9, "G"},
                                                                                    {6, "meg"},
                                                                                    {3, "k"},
                                                                                    {0, ""},
                                                                                    {-3, "m"},
                                                                                    {-6, "u"},
                                                                                    {-9, "n"},
                                                                                    {-12, "p"},
                                                                                    {-15, "a"}
                                                                                    };

       // static 

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            // Check whether this is an appropriate callback              
            if (!this.Equals(formatProvider))
                return null;

            // Set default format specifier              
            if (string.IsNullOrEmpty(format))
                format = "N";

            //string numericString = arg.ToString();
            double number = double.Parse((string)arg);
            int pow = (int)Math.Ceiling(Math.Log10(number));
            if (format == "N")
            {
                //if (numericString.Length <= 4)
                //    return numericString;
                //else if (numericString.Length == 7)
                //    return numericString.Substring(0, 3) + "-" + numericString.Substring(3, 4);
                //else if (numericString.Length == 10)
                //    return "(" + numericString.Substring(0, 3) + ") " +
                //           numericString.Substring(3, 3) + "-" + numericString.Substring(6);
                if (true)
                {
                    int pow2 = pow % 3;
                    double number2 = 0;
                    if (multiplies.ContainsKey(pow2))
                    {
                        number2 = number * Math.Pow(10, -pow2);
                    }
                    else
                        number2 = number * Math.Pow(10, -pow2);

                    return number2.ToString() + multiplies[pow2];
                }
                else
                    throw new FormatException(
                              string.Format("'{0}' cannot be used to format {1}.",
                                            format, arg.ToString()));
            }
            else
            {
                throw new FormatException(string.Format("The {0} format specifier is invalid.", format));
            }
            return Double.NaN.ToString(); ;
        }
    }



    public abstract class StringUtils
    {
        static Dictionary<int, string> multiplies = new Dictionary<int, string>() { 
                                                                                    {4, "T"},
                                                                                    {3, "G"},
                                                                                    {2, "meg"},
                                                                                    {1, "k"},
                                                                                    {-1, "m"},
                                                                                    {-2, "u"},
                                                                                    {-3, "n"},
                                                                                    {-4, "p"},
                                                                                    {-5, "a"},
                                                                                    {-6, "f"}
                                                                                    };
        /// <summary>
        /// Given a number, convert its to engineering notation, p.e: 
        /// 160,000 = 160K
        /// </summary>
        /// <param name="number">the numbre to convert to...</param>
        /// <returns></returns>
        public static string CodeString(double number)
        {
            if (number == 0)
                return number.ToString();

            int pow = (int)Math.Floor(Math.Log10(Math.Abs( number)));

            
            int pow2 = pow / 3;
            double number2 = 0;
            if (multiplies.ContainsKey(pow2))
            {
                number2 = number * Math.Pow(10, -pow2 * 3);
                return number2.ToString("0.00") + multiplies[pow2];
            }
            else if (multiplies.ContainsKey((pow - 1) / 3))
            {
                number2 = number * Math.Pow(10, -(pow2 - 1) * 3);
                return number2.ToString("0.00") + multiplies[pow2 - 1];
            }
            else if (pow > 15)
            {
                number2 = number * Math.Pow(10, -(pow - 1));
                return number2.ToString("0.00") + "E" + (pow - 1).ToString();
            }
            else if (pow < -15)
            {
                number2 = number * Math.Pow(10, -(pow - 1));
                return number2.ToString("0.00") + "E" + (pow - 1).ToString();
            }
            else
                return number.ToString("0.00");
        }

        /// <summary>
        /// converts strings representing numbers expressed 
        /// in Engieniring formar, just like:
        /// 1K2
        /// 12.2Meg
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DecodeString(string measure, out double value)
        {
            //|
            measure = measure.ToLower();
            var re = new Regex(@"(?i)(?<value>-?[\d.]+)\s*(?<multiplier>(meg)|[kgtmunp]?)\s*(?<unit>[avw]*)");
            var conversionFactors = new Dictionary<string, double> {
                                { "", 1 },
                                { "k", 1E3 },
                                { "meg", 1E6 },
                                { "g", 1E9 },
                                { "t", 1E12 },
                                { "m", 1E-3 },
                                { "u", 1E-6 },
                                { "n", 1E-9 },
                                { "p", 1E-12 },
                                { "a", 1E-15 },
                                { "f", 1E-18 },
            };

            var m = re.Match(measure);

            value = Convert.ToDouble(m.Groups["value"].Value);
            value *= conversionFactors[m.Groups["multiplier"].Value.ToLower()];

            return true;
        }

       
        [Obsolete]
        public static double DecodeString(string measure)
        {
            //|
            var re = new Regex(@"(?i)(?<value>-?[\d.]+)\s*(?<multiplier>(meg)|[kgtmunp]?)\s*(?<unit>[avw]*)");
            var conversionFactors = new Dictionary<string, double> {
                                { "", 1 }, 
                                { "k", 1E3 }, 
                                { "meg", 1E6 }, 
                                { "g", 1E9 }, 
                                { "t", 1E12 }, 
                                { "m", 1E-3 }, 
                                { "u", 1E-6 }, 
                                { "n", 1E-9 }, 
                                { "p", 1E-12 } 
            };

            var m = re.Match(measure);

            double value = Convert.ToDouble(m.Groups["value"].Value);
            return value * conversionFactors[m.Groups["multiplier"].Value.ToLower()];

        }
    }
}
