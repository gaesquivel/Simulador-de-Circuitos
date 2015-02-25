﻿using ElectricalAnalysis.Components;
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
        public Dictionary<double, Dictionary<string, Complex32>> Results { get; protected set; }

        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            int fila = 0;
            List<Node> nodos = new List<Node>();
            Results = new Dictionary<double, Dictionary<string, Complex32>>();

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

            ACAnalysis analis = ana as ACAnalysis;
            double w, wi, wf, deltaw, wx, pow = 1;

            wi = StringUtils.DecodeString(analis.StartFrequency);
            wf = StringUtils.DecodeString(analis.EndFrequency);
            //wf = 1E5;//StringUtils.DecodeString(analis.EndFrequency);
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
                nortoncopia.Clear();
                foreach (var item in nodosnorton)
                {
                    nortoncopia.Add(item);
                }
                
                //Calculo de tensiones de nodos
                Complex32 W = new Complex32(0, (float)w);
                var x = Calculate(nortoncopia, W);

                // #region 
                //#endregion        
                Dictionary<string, Complex32> result = new Dictionary<string, Complex32>();

                #region almacenamiento temporal
                foreach (var nodo in nortoncopia)
                {
                    fila = nortoncopia.IndexOf(nodo);
                    nodo.Voltage = x[fila];
                    result.Add(nodo.Name, nodo.Voltage);
                }
                foreach (var nodo in nodos)
                {
                    //if (nodosnorton.Contains(nodo))
                    //    continue;
                    result.Add(nodo.Name, nodo.Voltage);
                    //result.Add(
                }
                if (!Results.ContainsKey(w))
                    Results.Add(w, result);
                #endregion

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
            //calculo las corrientes:
            CalculateCurrents(cir);

            //ExportToCSV();
            return true;
        }

        protected static Vector<Complex32> Calculate(List<Node> nodosnorton, Complex32 W)
        {
            //existen nodos donde la tension se puede calcular directamente
            List<Node> nodosCalculables = new List<Node>();
            foreach (var nodo in nodosnorton)
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
                    nodosCalculables.Add(nodo);
                }
            }
            //remuevo esos nodos de los que deben calcularse matricialmente
            foreach (var nodo in nodosCalculables)
            {
                nodosnorton.Remove(nodo);
            }
            if (nodosnorton.Count == 0)
                return null;

            int fila = 0, columna = 0;
            var v = Vector<Complex32>.Build.Dense(nodosnorton.Count);
            var A = Matrix<Complex32>.Build.DenseOfArray(new Complex32[nodosnorton.Count, nodosnorton.Count]);

            foreach (var nodo in nodosnorton)
            {
                columna = fila = nodosnorton.IndexOf(nodo);
                Complex32 Z, V;
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                    nodo.TypeOfNode == Node.NodeType.InternalBranchNode)
                {
                    foreach (var rama in nodo.Components)
                    {
                        if (rama is Branch || rama is PasiveComponent)
                        {
                            V = rama.NortonCurrent(nodo, W);
                            v[fila] += V;
                            Z = rama.Impedance(W).Reciprocal();
                            A[fila, columna] += Z;
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
            return x;
        }

        private static void NavigateBranch(Node nodo , Dipole compo1, Complex32 W, ref Complex32 v1, ref Complex32 z1)
        {
            Node nodo1 = nodo;

            while (!(nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode || nodo1.IsReference)) 
            {
                if (compo1 is PasiveComponent)
                {
                    z1 += compo1.Impedance(W);
                }
                else if (compo1 is VoltageGenerator)
                {
                    v1 = compo1.voltage(nodo1);
                }
                else
                {
                    throw new NotImplementedException();
                }
                nodo1 = compo1.OtherNode(nodo1);
                compo1 = nodo1.OtherComponent(compo1);
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
                List<string> results = new List<string>();
                results.Add("W");
                foreach (var item in Results)
                {
                    foreach (var node in item.Value)
                        results.Add(node.Key.ToString());
                    break;
                }
                writer.WriteRow(results);
                results.Clear();

                //for (int row = 0; row < Results.Count; row++)
                foreach(var item in Results)
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

        private static void CalculateCurrents(ComponentContainer cir)
        {

       
        }

    }
}