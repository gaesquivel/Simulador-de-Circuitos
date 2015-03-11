using ElectricalAnalysis.Components;
using ElectricalAnalysis.Components.Controlled;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class ACSweepSolver: DCSolver
    {

        //                  W                Nname    Nvoltage
        public virtual Dictionary<double, Dictionary<string, Complex32>> Voltages { get; protected set; }
        //                  W              CompoName   CompoCurrent
        public virtual Dictionary<double, Dictionary<string, Complex32>> Currents { get; protected set; }

        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            List<Node> nodos = new List<Node>();
            Voltages = new Dictionary<double, Dictionary<string, Complex32>>();
            Currents = new Dictionary<double, Dictionary<string, Complex32>>();

            //List<Node> nodosnorton;
            //List<Node> nodoscalculables;
            //List<SpecialComponentInfo> specialcomponents;
            SolveInfo solveinfo = new SolveInfo();

            PreAnalizeToSolve(cir, nodos, solveinfo);

            ACAnalysis analis = ana as ACAnalysis;
            double w, wi, wf, deltaw, wx, pow = 1;

            wi = StringUtils.DecodeString(analis.StartFrequency);
            wf = StringUtils.DecodeString(analis.EndFrequency);
            w = wi;
            int i = (int)Math.Log10(wi) + 1;
            wx = Math.Pow(10, i);
            if (analis.ScanType == ACAnalysis.ACAnalysisScan.Linear)
                deltaw = (wf - wi) / analis.Points;
            else
            {
                deltaw = 1.0d / analis.Points;
                pow = Math.Pow(10, deltaw);
            }

            while (w < wf)
            {
                //Calculo de tensiones de nodos
                Complex32 W = new Complex32(0, (float)w);
                Calculate(solveinfo, W);

                Dictionary<string, Complex32> result = new Dictionary<string, Complex32>();

                #region almacenamiento temporal
 
                foreach (var nodo in nodos)
                {
                    result.Add(nodo.Name, nodo.Voltage);
                }

                if (!Voltages.ContainsKey(w))
                    Voltages.Add(w, result);
                #endregion

                //calculo las corrientes:
                CalculateCurrents(cir, W);
                Dictionary<string, Complex32> currents = new Dictionary<string, Complex32>();
                StorageCurrents(cir, currents);
                Currents.Add(w, currents);


                if (analis.ScanType == ACAnalysis.ACAnalysisScan.Linear)
                    w += deltaw;
                else
                {
                    w = w * pow;
                    if (w > 0.95 * wx)
                    {
                        i++;
                        wi = wx;
                        wx = Math.Pow(10, i);
                        w = wi;
                    }
                }
            }

            cir.State = Circuit.CircuitState.Solved;
            return true;
        }

        protected static void PreAnalizeToSolve(Circuit cir, List<Node> nodos, SolveInfo solveinfo)
        {
            nodos.AddRange(cir.Nodes.Values);
            nodos.Remove(cir.Reference);

            //nodosnorton = new List<Node>();
            //nodoscalculables = new List<Node>();
            //especialcomponents = new List<SpecialComponentInfo>();

            //guardo para futuro los nodos de los componentes especiales
            if (cir.OriginalCircuit != null)
            {
                foreach (var comp in cir.OriginalCircuit.Components)
                {
                    if (comp is ControlledDipole)
                    {
                        foreach (var nodo in comp.Nodes)
                        {
                            if (!nodo.IsReference)
                            {
                                SpecialComponentInfo info = new SpecialComponentInfo(comp);
                                info.ImportantOutputNodes.Add(nodo);
                                solveinfo.specialcomponents.Add(info);
                                if (!solveinfo.nortonnodes.Contains(nodo))
                                    solveinfo.nortonnodes.Add(nodo);
                                //especialcomponents.Add(nodo, comp);
                            }
                        }
                    }
                }
            }


            foreach (var nodo in nodos)
            {
                //los nodos de salida de un dispositivo VcV deben resolverse mediante matrices
                //foreach (var comp in especialcomponents)
                //{
                //    if ((comp.Component is ControlledVoltageGenerator && comp.Component.Nodes.Contains(nodo)))
                //    {
                //        nodosnorton.Add(nodo);
                //        continue;
                //    }
                //}
                if (solveinfo.nortonnodes.Contains(nodo))
                    continue;
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode
                    )
                    solveinfo.nortonnodes.Add(nodo);
                else if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode ||
                        nodo.TypeOfNode == Node.NodeType.VoltageFixedNode)
                {
                    solveinfo.calculablenodes.Add(nodo);
                }
            }
        }

        /// <summary>
        /// Almacena las corrientes del W actual
        /// </summary>
        /// <param name="cir"></param>
        /// <param name="currents"></param>
        private static void StorageCurrents(ComponentContainer cir, Dictionary<string, Complex32> currents)
        {
            foreach (var compo in cir.Components)
            {
                currents.Add(compo.Name, compo.current);
                if (compo is ComponentContainer)
                {
                    StorageCurrents((ComponentContainer)compo, currents);
                }
            }
        }

        protected static void Calculate(SolveInfo solveinfo, Complex32 W)
        {

            if (solveinfo.nortonnodes.Count == 0 && solveinfo.calculablenodes.Count == 0)
                return;

             //existen nodos donde la tension se puede calcular directamente
            foreach (var nodo in solveinfo.calculablenodes)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageFixedNode)
                {
                    foreach (var compo in nodo.Components)
                    {
                        if (compo.IsConnectedToEarth && (compo is ACVoltageGenerator))
                        {
                            nodo.Voltage = compo.voltage(nodo, W);   //el componente conectado a tierra debe ser Vdc o Vsin o capacitor
                            break;
                        }
                    }
                    continue;
                }
            }

            #region Calculo de tensions mediante matrices

            if (solveinfo.nortonnodes.Count > 0)
            {
                int fila = 0, columna = 0, max;
                //cada componente especial agrega 0, 1 o 2 variable mas, dependiendo del tipo
                max = solveinfo.MatrixDimension;
                var v = Vector<Complex32>.Build.Dense(max);
                var A = Matrix<Complex32>.Build.DenseOfArray(new Complex32[max, max]);

                //primero la ecuacion de transferencia del VcV
                foreach (var info in solveinfo.specialcomponents)
                {
                    //foreach (var item in info.Component)
                    //{
                        
                    //}
                    //int entrada = especialcomponents.IndexOf(info);
                    //fila = nodosnorton.Count - 1 + entrada;
                    columna = 1;
                    A[fila, columna] = 1;


                }

                foreach (var nodo in solveinfo.nortonnodes)
                {
                    columna = fila = solveinfo.nortonnodes.IndexOf(nodo);
                    Complex32 Z, V;
                    if (
                        nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                        nodo.TypeOfNode == Node.NodeType.VoltageFixedNode ||
                        nodo.TypeOfNode == Node.NodeType.InternalBranchNode)
                    {
                        foreach (var rama in nodo.Components)
                        {
                            if (rama is Branch || rama is PasiveComponent)
                            {
                                columna = fila = solveinfo.nortonnodes.IndexOf(nodo);
                                if (rama is Branch)
                                    V = ((Branch)rama).NortonCurrent(nodo, W);
                                else
                                    V = rama.Current(nodo, W);
                                
                                v[fila] += V;
                                Z = rama.Impedance(W);
                                A[fila, columna] += 1 / Z;
                                Node nodo2 = rama.OtherNode(nodo);
                                if (!nodo2.IsReference)
                                {
                                    columna = solveinfo.nortonnodes.IndexOf(nodo2);
                                    A[fila, columna] -= 1 / Z;
                                }
                            }
                            else if (rama is CurrentGenerator)
                            {
                                V = rama.Current(nodo, W);
                                v[fila] += V;
                            }
                            else if (rama is VoltageControlledGenerator)
                            {
                                //int entrada = especialcomponents.Values.ToList().IndexOf(rama);
                                //columna = nodosnorton.Count -1 + entrada;

                                ////un nodo es positva la corriente, en el otro es negativa 
                                //if (entrada % 2 == 0)
                                //    A[fila, columna] = 1;
                                //else
                                //    A[fila, columna] = -1;
                                
                            }
                        }
                    }
                    else if (nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode)
                    {
                        Dipole compo = nodo.Components[0];
                        if (!(compo is VoltageGenerator))
                            compo = nodo.Components[1];
                        v[fila] = compo.Voltage.Real;
                        A[fila, columna] = 1;
                        columna = solveinfo.nortonnodes.IndexOf(compo.OtherNode(nodo));
                        A[fila, columna] = -1;
                    }
                    //else if (nodo.TypeOfNode == Node.NodeType.VoltageFixedNode)
                    //{
                    //    foreach (var rama in nodo.Components)
                    //    {
                    //        if (rama is VoltageControlledGenerator)
                    //        {

                    //        }
                    //    }
                    //}
                    else
                        throw new NotImplementedException();
                }
                var x = A.Solve(v);

                foreach (var nodo in solveinfo.nortonnodes)
                {
                    fila = solveinfo.nortonnodes.IndexOf(nodo);
                    nodo.Voltage = x[fila];
                }
            }

            #endregion

            //existen nodos donde la tension se puede calcular indirectamente
            foreach (var nodo in solveinfo.calculablenodes)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                {
                    Node nodo1 = nodo;
                    Dipole compo1 = nodo.Components[0];
                    Branch br = compo1.Owner as Branch;
                    if (br == null)
                    {
                        throw new Exception();
                    }
                    Complex32 v1, v2, z1, z2;
                    v1 = v2 = z1 = z2 = Complex32.Zero;
                    //el nodo es interno en una rama, deberian los 2 componentes conectados
                    //al nodo, 
                    Dipole compo2 = nodo.Components[1];
                    //hacia la izq (o derec)
                    NavigateBranch(nodo1, compo1, W, ref v1, ref z1);
                    //hacia el otro lado
                    NavigateBranch(nodo1, compo2, W, ref v2, ref z2);
                    nodo.Voltage = (z2 * v1 + v2 * z1) / (z1 + z2);
                }
            }


        }

        /// <summary>
        /// barre una rama desde un nodo interno hacia la izquierda o derecha
        /// calculando la suma de las tensiones de generadores V1
        /// y la suma de impedancias Z1
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="compo1"></param>
        /// <param name="W"></param>
        /// <param name="v1"></param>
        /// <param name="z1"></param>
        private static void NavigateBranch(Node nodo , Dipole compo1, Complex32 W, ref Complex32 v1, ref Complex32 z1)
        {
            Node nodo1 = nodo;

            while (!(nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode || nodo1.IsReference)) 
            {
                if (nodo1.TypeOfNode == Node.NodeType.VoltageFixedNode)
                {
                    //llegue al final de la rama
                    v1 += nodo1.Voltage;
                    break;
                }
                if (compo1 is PasiveComponent)
                {
                    z1 += compo1.Impedance(W);
                }
                else if (compo1 is VoltageGenerator)
                {
                    v1 += compo1.voltage(nodo1, W);
                }
                else
                {
                    throw new NotImplementedException();
                }
                nodo1 = compo1.OtherNode(nodo1);
                compo1 = nodo1.OtherComponent(compo1);
            }
            if (nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode && !nodo1.IsReference)
            {
                v1 += nodo1.Voltage;
            }
        }

        public override void ExportToCSV(string FileName)
        {

            //se guarda@)
            //W     N1  N2 .... Nn
            //10    5   15      2
            //12    6   14      1
            //List<List<string>> todos = new List<List<string>>();
            using (var writer = new CsvFileWriter(FileName))
            {
                writer.Delimiter = ';';
                List<string> results = new List<string>();
                results.Add("W");
                foreach (var item in Voltages)
                {
                    foreach (var node in item.Value)
                        results.Add(node.Key.ToString());
                    break;
                }
                writer.WriteRow(results);
                results.Clear();

                //for (int row = 0; row < Results.Count; row++)
                foreach(var item in Voltages)
                {
                    results.Add(item.Key.ToString());
                    foreach (var node in item.Value)
                    {
                        results.Add(node.Value.ToString());
                    }
                    writer.WriteRow(results);
                    results.Clear();
                }
            }

        }

        protected static void CalculateCurrents(ComponentContainer container, Complex32 W)
        {

            foreach (var comp in container.Components)
            {
                //la corriente en las resistencias se calcula directamente en ellas: es ley de Ohm:V/R
                if (comp is PasiveComponent || comp is CurrentGenerator)
                {
                    Node nodo = comp.Nodes[0];
                    comp.current = comp.Current(nodo, W);
                    if (container is Branch)
                    {
                        ((Branch)container).current = comp.current;
                    }
                    continue;
                }


                //en los Generadores de tension hay que calcular la corriente en 1 u ambos nodos
                if (comp is VoltageGenerator)
                {
                    foreach (var nodo in comp.Nodes)
                    {
                        Dipole comp2 = nodo.OtherComponent(comp);
                        //si tiene solo un resistor en serie es automatico el valor de corriente
                        if (nodo.Components.Count == 2 && (comp2 is PasiveComponent || comp2 is CurrentGenerator))
                        {
                            comp.current = comp2.Current(nodo, W);

                            if (container is Branch)
                                ((Branch)container).current = comp.current;
                            goto out1;
                        }
                    }
                    //si no tiene solo una resistencias en serie, es decir, un nodo de multiples ramas
                    //se aplica 2da de Kirchoff para el supernodo
                   // throw new NotImplementedException();
                    foreach (var nodo in comp.Nodes)
                    {
                        if (nodo.IsReference)
                        {
                            continue;
                        }
                        Complex32 i = Complex32.Zero;
                        i =  CalculateSupernodeCurrent(nodo, W, comp);
                        comp.current = i;
                        break;
                    }
                }
                else if (comp is Branch)
                {
                    CalculateCurrents((Branch)comp, W);
                    continue;
                }

                else if (comp is ParallelBlock)
                {
                    CalculateCurrents((ParallelBlock)comp, W);
                    continue;
                }

            out1: ;
            }
        }

        /// <summary>
        /// Dado un supernodo, recorre todas sus ramas para hallar la corriente en una de ellas
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="W"></param>
        /// <returns></returns>
        private static Complex32 CalculateSupernodeCurrent(Node nodo, Complex32 W, Dipole comp)
        {
            Complex32 i = Complex32.Zero;
            foreach (var comp2 in nodo.Components)
            {
                if (comp2 == comp)
                {
                    continue;
                }
                if (comp2 is VoltageGenerator)
                {
                    Node nodo1 = comp2.OtherNode(nodo);
                    i += CalculateSupernodeCurrent(nodo1, W, comp2);
                }else
                    i += comp2.Current(nodo, W);
            }
            return i;
        }
    }
}
