using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using ElectricalAnalysis.Laplace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace ElectricalAnalysis.Components.Controlled
{
    public class LaplaceVoltageGenerator: VoltageControlledGenerator
    {

        ObservableCollection<Singularity> singularities;
        /// <summary>
        /// Collection of Poles and Zeroes
        /// </summary>
        public ObservableCollection<Singularity> Singularities
        {
            get { return singularities ?? (singularities = new ObservableCollection<Singularity>()); }
        }

        protected override string DefaultName { get { return "Laplace"; } }

        public override string Expresion
        {
            get { return base.Expresion; }
            set
            {
                base.Expresion = value;
                Parse(value);
            }
        }

        private bool Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (Singularities.Count > 0)
                return true;
            //Singularities.Add(new Singularity(new Complex(-100, 0)));
            //Singularities.Add(new Singularity(new Complex(-1E4, 0)));
            //Singularities.Add(new Singularity(new Complex(-1E5, 0)));
            //Gain = "100K";
            //return true;
            try
            {
                //E_LAPLACE1  out 0 LAPLACE { V(s)}  { (1) / (-100 + s)}

                //{V(in)} {(N) / (D)}
                //(N) o (D) = ((A + s)(B + s)...)
                //(1) / (-100 + s)
                string tmp = value.Replace(" ", "").ToLower();
                if (!tmp.StartsWith("{") || !tmp.EndsWith("}"))
                {
                    return false;
                }
                tmp = value.Replace("{", "");
                string[] arr = tmp.Split("}".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arr == null || arr.Length != 2)
                {
                    return false;
                }
                //V(in)
                //(N) / (D)
                //arr[0].Remove(arr[0].Length - 1);
                //arr[1].Substring(1);
                string[] num = null;
                if (arr[1].Contains("/"))
                    num = arr[1].Split('/');

                if (num.Length > 2 || num.Length <= 0)
                {
                    return false;
                }
                double K = 1;
                List<Singularity> zeros = new List<Singularity>(); 
                ParseSingularities(num[0], zeros, ref K, true);

                //List<Singularity> poles = new List<Singularity>();
                Gain = K.ToString();
                if (num.Length == 2)
                    ParseSingularities(num[1], zeros, ref K, false);

                foreach (var item in zeros)
                    Singularities.Add(item);
                //Singularities = zeros;
                return true;
            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(new Notification(ex));
            }
            return false;
        }

        public static bool ParseSingularities(string expression, 
                                                List<Singularity> sings,
                                                ref double constant,
                                                bool isnumerator = false
                                                )
        {
            //must parse:   (()()()...())     or  (...)
            expression = expression.Replace(" ", "").ToLower();
            if (expression.StartsWith("((") && expression.EndsWith("))"))
                expression = expression.Substring(1).Remove(expression.Length - 1);
            else if (expression.StartsWith("(") && expression.EndsWith(")"))
            {
                expression = expression.Substring(1);
                expression = expression.Remove(expression.Length - 1);
            }
            else
                NotificationsVM.Instance.Notifications.Add(
                                                    new Notification("Unknown expression " + expression,
                                                                        Notification.ErrorType.error));

            //queda ()()()... o s+a
            string[] arr = expression.Split("(".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 1)
            {
                double cons = 0;
                if (double.TryParse(expression, out cons))
                {
                    constant = cons;
                    return true;
                }
                //notificationsvm.instance.notifications.add(
                //                                    new notification("unknown expression " + expression,
                //                                                        notification.errortype.error));
                //return false;
            }
            //queda cada item como: k+-s o s+-k
            foreach (var item in arr)
            {
                //if (item.EndsWith(")"))
                //    item.Remove(item.Length - 1);
                //else
                //{
                //    NotificationsVM.Instance.Notifications.Add(new Notification("Error on try parse expression: " + item));
                //    return false;
                //}
                char plus = '+';
                double num = 0;
                if (item.Contains("-"))
                    plus = '-';
                string[] dos = item.Split(plus);
                if(dos.Length == 2)
                {
                    if (dos[0] == "s")
                    {
                        if (!double.TryParse(dos[1], out num))
                            return false;
                    }
                    if (dos[1] == "s")
                        if (!double.TryParse(dos[1], out num))
                            return false;
                }
                else
                {
                    NotificationsVM.Instance.Notifications.Add(new Notification("Error on try parse expression: " + item));
                    return false;
                }
                Singularity.SingularityTypes singtype = Singularity.SingularityTypes.Pole;
                if (isnumerator)
                    singtype = Singularity.SingularityTypes.Zero;

                sings.Add(new Singularity(new Complex(num, 0), 1, singtype));
            }


            return true;
        }

        public LaplaceVoltageGenerator(ComponentContainer owner, string name) :base(owner, name)
        {

        }

        public override Complex voltage(NodeSingle referenceNode, Complex? W = default(Complex?))
        {
            Complex v = InputNodes[0].Voltage;
            if (InputNodes.Count > 1)
            {
                //si no esta una entrada a tierra hay que hacer la diferencia
                v -= InputNodes[1].Voltage;
            }
            v = v * new Complex(gain, 0);
            foreach (var sing in Singularities)
            {
                if (sing.SingularityType == Singularity.SingularityTypes.Pole)
                {
                    if ((W.Value - sing.Value) == Complex.Zero)
                    {
                        v = new Complex(double.PositiveInfinity, double.PositiveInfinity);
                        NotificationsVM.Instance.Notifications.Add(new Notification("Pole detected at " + sing.ToString()));
                        break;
                    }
                    v = v / (W.Value - sing.Value);
                }
                else
                    v = v * (W.Value - sing.Value);
            }
            if (referenceNode == Nodes[0])
                return v;
            else if (referenceNode == Nodes[1])
                return -v;
            else
                return Double.NaN; 
        }

        public override Complex ControllerEquationValue(object node, object e, bool isinput = false)
        {
            if (node is NodeSingle)
            {
                NodeSingle nodo1 = node as NodeSingle;
                if (Nodes.Contains(nodo1) && !isinput)
                {
                    if (Nodes[0] == nodo1)
                        return 1;
                    else
                        return -1;
                }
                else if (InputNodes.Contains(nodo1) && isinput)
                {
                    Complex g = gain;
                    Complex W = Complex.Zero;
                    if (e != null)
                        W = (Complex)e;
                    foreach (var sing in Singularities)
                    {
                        if (sing.SingularityType == Singularity.SingularityTypes.Pole)
                        {
                            if ((W - sing.Value) == Complex.Zero)
                            {
                                g = new Complex(double.PositiveInfinity, double.PositiveInfinity);
                                //Notifications.Add(new Notification("Pole detected at " + sing.ToString()));
                                break;
                            }
                            g = g * sing.Value / (W - sing.Value);
                        }
                        else
                            g = g * (W - sing.Value) / sing.Value;
                    }
                    if (InputNodes[0] == nodo1)
                        return -g;
                    else
                        return g;
                }
                else
                    throw new NotImplementedException();
            }
            else if (node is int && (int)node == 0)
            {
                return Complex.Zero;
            }
            throw new NotImplementedException();
        }

    }
}
