using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using ElectricalAnalysis.Laplace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ElectricalAnalysis.Components.Controlled
{
    public class LaplaceVoltageGenerator: VoltageControlledGenerator //, Parseable
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

        private bool Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;
            
            try
            {
                //E_LAPLACE1  out 0 LAPLACE { V(in+) }  { (1) / (-100 + s)}

                //{in1 in2} {(N) / (D)}
                //(N) o (D) = ((A + s)(B + s)...)
                //(1) / (-100 + s)
                string inputs = "", aux = "";
                if (!ParseLaplaceExpression(expression, ref inputs, ref aux))
                    return false;

                if (string.IsNullOrWhiteSpace(inputs))
                    return false;

                if (inputs.StartsWith("{") && inputs.EndsWith("}"))
                    inputs = inputs.Remove(inputs.Length - 1).Substring(1);

                InputNodes.Clear();
                Node node1 =  null, node2 = null;
                string[] arr = inputs.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in OwnerCircuit.Nodes.Values)
                {
                    if (item.Name == arr[0])
                    {
                        node1 = item;
                    }
                    else if (arr.Length == 2 && item.Name == arr[1])
                    {
                        node2 = item;
                    }
                }


                //node1 = CreateOrFindNode("in+");
                if (node1 == null && node2 == null)
                    return false;
                if (node1 == null)
                {
                    node1 = OwnerCircuit.Reference;
                }
                if (node2 == null)
                {
                    node2 = OwnerCircuit.Reference;
                }

                InputNodes.Add((NodeSingle)node1);
                InputNodes.Add((NodeSingle)node2);
              

                string num = "", den = "";
                if (!MathUtil.FindFraction(aux, ref num, ref den))
                    return false;

                if (!MathUtil.ValidateParenthesys(num))
                    return false;
                if (!MathUtil.ValidateParenthesys(den))
                    return false;

                List<string> sings = new List<string>();
                if (!MathUtil.FindSingularities(num, ref sings))
                {
                    //del tipo 
                    //Complex c = Complex.Zero;
                    double c = 0;
                    if (num.StartsWith("(") && num.EndsWith(")"))
                        num = num.Remove(num.Length-1).Substring(1);
                    if (!MathUtil.FindNumber(num, ref c))
                        return false;
                    Gain = c.ToString();
                }
                //tengo los zeros
                Complex valor = Complex.Zero;
                foreach (var zero in sings)
                {
                    if (!MathUtil.FindSingularityValue(zero, ref valor))
                        return false;
                    Singularities.Add(new Singularity(valor, 1, 
                                        Singularity.SingularityTypes.Zero));
                }

                sings.Clear();
                if (!MathUtil.FindSingularities(den, ref sings))
                    return false;

                foreach (var polo in sings)
                {
                    if (!MathUtil.FindSingularityValue(polo, ref valor))
                        return false;
                    Singularities.Add(new Singularity(valor, 1,
                                        Singularity.SingularityTypes.Pole));
                }
                
                return true;
            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(new Notification(ex));
            }
            return false;
        }

        /// <summary>
        /// try to parse an sub expression like from voltage generator like:
        /// E_LAPLACE1  out 0 LAPLACE { V(in+) }  { (1) / (-100 + s)}
        /// </summary>
        /// <param name="expression">An expression like: { in1  in2 }  { (1) / (-100 + s)}</param>
        /// <param name="inputs">returns:   in1  in2</param>
        /// <param name="laplaceexpression">returns:   (1) / (-100 + s)</param>
        /// <returns>true if could be parsed, else returns false</returns>
        private bool ParseLaplaceExpression(string expression, 
                                            ref string inputs,
                                            ref string laplaceexpression)
        {
            string regexPattern = @"\{(?<input>.+)\}\s*\{(?<valor>.+)\}";
            Regex regex = new Regex(regexPattern);
            Match match = regex.Match(expression);
            if (match.Groups.Count > 0)
            {
                inputs = match.Groups["input"].Value;
                laplaceexpression = match.Groups["valor"].Value;
                return true;
            }
            return false;
        }

        [Obsolete]
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
                        if (e is double)
                            //W = new Complex(0, (double)e);
                            throw new NotImplementedException("Transient for Laplace block remains unresolved!");
                            //habria que ccalcular la antitransformada
                        else
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
