using ElectricalAnalysis.Components;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class TransientSolver: CircuitSolver
    {
        //                  time             nodes voltages
        public Dictionary<double, Dictionary<string, double>> Voltages { get; protected set; }
        public Dictionary<double, Dictionary<string, double>> Currents { get; protected set; }

        public bool Solve(Components.Circuit cir, BasicAnalysis ana)
        {
            List<Node> nodos = new List<Node>();
            Voltages = new Dictionary<double, Dictionary<string, double>>();
            Currents = new Dictionary<double, Dictionary<string, double>>();

            nodos.AddRange(cir.Nodes.Values);
            nodos.Remove(cir.Reference);

            List<Node> nodosnorton = new List<Node>();
            List<Node> nortoncopia = new List<Node>();

            foreach (var nodo in nodos)
            {
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                    nodosnorton.Add(nodo);
            }

            TransientAnalysis analis = ana as TransientAnalysis;
            double t, wi, tf, deltat, wx;

            deltat = StringUtils.DecodeString(analis.Step);
            tf = StringUtils.DecodeString(analis.FinalTime);
            t = 0;
       
            while (t < tf)
            {
                nortoncopia.Clear();
                foreach (var item in nodosnorton)
                {
                    nortoncopia.Add(item);
                }

                //Calculo de tensiones de nodos
                Calculate(nortoncopia, t);

                Dictionary<string, double> result = new Dictionary<string, double>();

                #region almacenamiento temporal

                foreach (var nodo in nodos)
                {
                    result.Add(nodo.Name, nodo.Voltage.Real);
                }

                if (!Voltages.ContainsKey(t))
                    Voltages.Add(t, result);
                #endregion

                //calculo las corrientes:
                //CalculateCurrents(cir, W);
                //Dictionary<string, Complex32> currents = new Dictionary<string, Complex32>();
                //StorageCurrents(cir, currents);
                //Currents.Add(t, currents);


                t += deltat;
            }

            return true;
        }

        protected static void Calculate(List<Node> nodosnorton, double t)
        {

            if (nodosnorton.Count == 0)
                return;

            //existen nodos donde la tension se puede calcular directamente
            List<Node> nodosCalculables = new List<Node>();

            foreach (var nodo in nodosnorton)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                    nodosCalculables.Add(nodo);
            }
            foreach (var nodo in nodosCalculables)
                nodosnorton.Remove(nodo);

            if (nodosnorton.Count > 0)
            {

                int fila = 0, columna = 0;
                var v = Vector<double>.Build.Dense(nodosnorton.Count);
                var A = Matrix<double>.Build.DenseOfArray(new double[nodosnorton.Count, nodosnorton.Count]);

                foreach (var nodo in nodosnorton)
                {
                    columna = fila = nodosnorton.IndexOf(nodo);
                    double Z, V;
                    if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                        nodo.TypeOfNode == Node.NodeType.InternalBranchNode)
                    {
                        foreach (var rama in nodo.Components)
                        {
                            if (rama is Branch || rama is Resistor)
                            {
                                columna = fila = nodosnorton.IndexOf(nodo);
                                if (rama is Branch)
                                    V = ((Branch)rama).NortonCurrent(nodo, t);
                                else
                                    V = rama.Current(nodo, t);
                                
                                v[fila] += V;
                                Z = rama.Impedance().Real;
                                A[fila, columna] += 1 / Z;
                                Node nodo2 = rama.OtherNode(nodo);
                                if (!nodo2.IsReference)
                                {
                                    columna = nodosnorton.IndexOf(nodo2);
                                    A[fila, columna] -= 1 / Z;
                                }
                            }
                            else if (rama is CurrentGenerator)
                            {
                                V = rama.Current(nodo, t);
                                v[fila] += V;
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
                        columna = nodosnorton.IndexOf(compo.OtherNode(nodo));
                        A[fila, columna] = -1;
                    }
                    else
                        throw new NotImplementedException();
                }
                var x = A.Solve(v);
                //return x;
                foreach (var nodo in nodosnorton)
                {
                    fila = nodosnorton.IndexOf(nodo);
                    nodo.Voltage = new Complex32((float)x[fila], 0);
                    //result.Add(nodo.Name, nodo.Voltage);
                }
            }


            //existen nodos donde la tension se puede calcular directamente
            foreach (var nodo in nodosCalculables)
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
                    double v1, v2, z1, z2;
                    v1 = v2 = z1 = z2 = 0;
                    //el nodo es interno en una rama, deberian los 2 componentes conectados
                    //al nodo, 
                    Dipole compo2 = nodo.Components[1];
                    //hacia la izq (o derec)
                    NavigateBranch(nodo1, compo1, t, ref v1, ref z1);
                    //hacia el otro lado
                    NavigateBranch(nodo1, compo2, t, ref v2, ref z2);
                    nodo.Voltage = new Complex32((float) ((z2 * v1 + v2 * z1) / (z1 + z2)),0);
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
        /// <param name="t"></param>
        /// <param name="v1"></param>
        /// <param name="z1"></param>
        private static void NavigateBranch(Node nodo, Dipole compo1, double t, ref double v1, ref double z1)
        {
            Node nodo1 = nodo;

            while (!(nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode || nodo1.IsReference))
            {
                if (compo1 is Resistor)
                {
                    z1 += compo1.Impedance().Real;
                }
                else if (compo1 is VoltageGenerator || compo1 is Capacitor)
                {
                    v1 += compo1.voltage(nodo1, t);
                }
                else
                {
                    throw new NotImplementedException();
                }
                nodo1 = compo1.OtherNode(nodo1);
                compo1 = nodo1.OtherComponent(compo1);
            }
            if (nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode)
            {
                v1 += nodo1.Voltage.Real;
            }
        }

        
    }
}
