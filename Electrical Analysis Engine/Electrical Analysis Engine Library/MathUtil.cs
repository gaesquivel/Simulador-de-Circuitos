using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

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


        /// <summary>
        /// Validate any math expression finding match for open and 
        /// closing parenthesys
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool ValidateParenthesys(string expression)
        {
            var regex = new Regex(@"^[^\(\)]*(((?'Open'\()[^\(\)]*)+((?'Close-Open'\))[^\(\)]*)+)*(?(Open)(?!))$");
            //(?x)^(?>(?<p>\()*(?>-?[\d|s]+(?:\.[\d|s]+)v?)(?<-p>\))*)(?>(?:[-+*/](?>(?<p>\()*(?>-?[\d|s]+(?:\.[\d|s]+)?)(?<-p>\))*))*)(?(p)(?!))$");
            //(?x)^(?>(?<p>\()*(?>-?\d+(?:\.\d+)v?)(?<-p>\))*)(?>(?:[-+*/](?>(?<p>\()*(?>-?\d+(?:\.\d+)?)(?<-p>\))*))*)(?(p)(?!))$");
            //\(([^()]|(?R))*\)
            return regex.IsMatch(expression);
        }

        public static bool ValidatePolinomial(string expression)
        {
            var regex = new Regex(@"(?<part>[+-]?(?<n>\d+(?:\.\d+)?)?(?(n)x?|x)(?:\^\d+)?)(?:[ ]*(?<op>[+-]|$))");


            return true;
        }

        /// <summary>
        /// Find into an expression one numerator y denominator
        /// of a fractional number or formulae
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public static bool FindFraction(string expression, 
                                        ref string numerator,
                                        ref string denominator)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;
            expression = expression.Replace(" ", "");

            string regexPattern = @"(?<numerador>.*)\/(?<denominador>.*)";
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(expression);
            //numerator = "";
            //denominator = "";
            if (match.Groups.Count > 0)
            {
                numerator = match.Groups[1].Value;
                denominator = match.Groups[2].Value;
            }
            else if (match.Groups.Count == 1)
                numerator = match.Groups[1].Value;
            else
                return false;
            return true;
        }

        /// <summary>
        /// Find like poles or zeroes expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="singularities"></param>
        /// <returns></returns>
        public static bool FindSingularities(string expression,
                                       ref List<string> singularities)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;
            expression = expression.Replace(" ", "");

            string regexPattern = @"\G\(.+\s?[-+]\s?s\)\*?|\G\(s\s?[-+]\s?.+\s?\)\*?";
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(expression);
            if (match.Success)
            {
                //string[] arr = new string[match.Groups.Count];
                //match.Groups.CopyTo(arr, 0);
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    singularities.Add(match.Groups[i].Value);
                }
                return true;
            }
                
            return false;
        }


        public static bool FindSingularityValue(string expression,
                                      ref Complex value)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;
            expression = expression.Replace(" ", "");

            string regexPattern = @"\((?<valor>[-+]?.+)+s\)|\(s(?<valor>[-+].+)\)";
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(expression);
            string valor = "";
            if (match.Success)
            {
                valor = match.Groups["valor"].Value;
                return (FindComplexNumber(valor, ref value));
            }

            regexPattern = @"\(s\s?(?<valor>[-+]\s?.+)\s?\)";
            regex = new Regex(regexPattern);
            match = regex.Match(expression);
            if (match.Success)
            {
                valor = match.Groups["valor"].Value;
                return (FindComplexNumber(valor, ref value));
            }

            return false;
        }


        public static bool FindNumber(string expression, ref double value)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;
            //private static Regex TermSplitterExpression =new Regex(
            //@"^(?<k_n>[0-9\.]*)\s*(/)?\s*(?<k_d>[0-9]*)\s*(?<x>x{0,1})\s*(\^\s*(?<exp_n>[0-9\.]+)\s*(/)?\s*(?<exp_d>[0-9]*))?$");

            // The pattern has been broken down for educational purposes
            string regexPattern =
            // Match any float, negative or positive, group it
            //@"^[0-9]*(?:\.[0-9]*)?$" +
            //-+ 0 o 1 vez
            @"([-+]?" +
            //numero al menos 1 vez, seguido de . (0 o 1 vez)
            @"\d+\.?\d*|[-+]?\d*\.?\d+)" +
            // ... possibly following that with whitespace
            @"\s*"
                ;
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(expression);
            double real = 0;
            //value = real;
            NumberFormatInfo info = new NumberFormatInfo();
            info.NumberDecimalSeparator = ".";
            if (!double.TryParse(match.Groups[1].Value, 
                NumberStyles.Float, info , out real))
                return false;

            value = real;
           
            return true;
        }

        public static bool FindComplexNumber(string expression, ref Complex value)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            string []regexPatterns = {
               /*-20.1i*/  @"(?<imag>[-+]?\d+\.?\d*|\d*\.?\d+)?(?<iunit>[ij])",
                /*-20.1*/  @"(?<iunit>[ij])?(?<real>[-+]?^[ij]\d+\.?\d*|[-+]?[^[ij]\d*\.?\d+)(?(iunit)[ij])?",
               ///*100.1i*/  @"(?<real>[-+]?^[ij]\d+\.?\d*|[-+]?[^[ij]\d*\.?\d+)",
               /*20.4i-100.1*/  @"[-+]?(?<imag>\d+\.?\d*|\d*\.?\d+)?[ij](?<real>[-+]?^[ij]\d+\.?\d*|[-+]?[^[ij]\d*\.?\d+)?",
               /*-33.4+100.1i*/ @"(?<real>[-+]?^[ij]\d+\.?\d*|[-+]?[^[ij]\d*\.?\d+)?(?<imag>[-+]?\d+\.?\d*|\d*\.?\d+)?[ij]"
            };
            #region previos
            
            #endregion

            double real = 0, imag = 0;
            foreach (var item in regexPatterns)
            {
                Regex regex = new Regex(item);
                Match match = regex.Match(expression);
                if (match.Success)
                {
                    NumberFormatInfo info = new NumberFormatInfo();
                    info.NumberDecimalSeparator = ".";
                    if (!double.TryParse(match.Groups["real"].Value, NumberStyles.Float, info, out real) &&
                        !double.TryParse(match.Groups["imag"].Value, NumberStyles.Float, info, out imag))
                        return false;
                    break;
                }
            }
            
            value = new Complex(real, imag);

            return true;
        }

        //public static bool FindSingularities(string expression, out Complex value)
        //{
        //    //(s+4)*(s+5)
        //    expression = expression.ToLower();
        //    var re = new Regex(@"(?i)(?<value>-?[\d.]+)\s*(?<multiplier>(meg)|[kgtmunp]?)\s*(?<unit>[avw]*)");
        //    var conversionFactors = new Dictionary<string, double> {
        //                        { "", 1 },
        //                        { "k", 1E3 },
        //                        { "meg", 1E6 },
        //                        { "g", 1E9 },
        //                        { "t", 1E12 },
                              
        //                        { "a", 1E-15 },
        //                        { "f", 1E-18 },
        //    };

        //    var m = re.Match(expression);

        //    value = Convert.ToDouble(m.Groups["value"].Value);
        //    value *= conversionFactors[m.Groups["multiplier"].Value.ToLower()];

        //    return true;
        //}

    }
}
