using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis.Components;
using System.Numerics;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;
using ElectricalAnalysis.Analysis.Solver;

namespace ElectricalAnalysis.Analysis
{
    public class ParametricAnalisys : BasicAnalysis
    {

        public const string Key = "*.param";

        /// <summary>
        /// Valores complejos que adoptara el parametro
        /// </summary>
        public List<Complex> Values { get; protected set; }
        public string Parameter { get; set; }

        public ParametricAnalisys()
        {
            Values = new List<Complex>();
            Solver = new ParametricSolver();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Parse(Circuit owner, string expression)
        {
            // en LTSPice es: .step param R list 5 10 50
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            expression = expression.ToLower();

            if (!expression.StartsWith(Key))
                return false;

            //.param R list 5 10 50
            //.param R 1 100 5
            // el parametro R es parte del valor de un componente, va entre llaves {R} 
            //string aux = expression.Substring(Key.Length);
            string[] arr = expression.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length < 4)
            {
                NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Unknown param expression: " + expression,
                                                            Notification.ErrorType.error));
                return false;
            }

            Parameter = arr[1];

            if (arr[2] == "list")
            {
                for (int i = 3; i < arr.Length; i++)
                {
                    Complex val = Complex.Zero;
                    double dol = 0;
                    //if (double.TryParse(arr[i], out dol))
                    if (StringUtils.DecodeString(arr[i], out dol))
                        Values.Add(new Complex(dol, 0));
                    else
                        NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Invalid number format: " + arr[i],
                                                            Notification.ErrorType.error));
                }
            }
            else if (arr.Length == 5)
            {
                //is linear 
                double min = 0, max = 0, step = 1;
                if (StringUtils.DecodeString(arr[2], out min) ||
                    StringUtils.DecodeString(arr[3], out max) ||
                    StringUtils.DecodeString(arr[4], out step))
                {
                    NotificationsVM.Instance.Notifications.Add(
                                new Notification("Invalid number format: " + expression,
                                                    Notification.ErrorType.error));
                    return false;
                }

                for (double i = min; i < max; i += step)
                {
                    Values.Add(new Complex(i, 0));
                }


            }
            else
            {
                NotificationsVM.Instance.Notifications.Add(
                                       new Notification("Invalid param format: " + expression,
                                                           Notification.ErrorType.error));
                return false;
            }

            Name = "Parametric " + Parameter;
            owner.Setup.Add(this);
            return true;
        }
    }
}
