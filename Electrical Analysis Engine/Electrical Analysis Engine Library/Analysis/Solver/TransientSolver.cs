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
        public Dictionary<double, Dictionary<string, double>> Results { get; protected set; }

        public bool Solve(Components.Circuit cir, BasicAnalysis ana)
        {
            int fila = 0, columna = 0;
            List<Node> nodos = new List<Node>();
            Results = new Dictionary<double, Dictionary<string, double>>();

            foreach (var nodo in cir.Nodes.Values)
            {
                //creo una lista de nodos sin el nodo referencia
                if (!nodo.IsReference)
                    nodos.Add(nodo);
            }

            List<Node> nodosnorton = new List<Node>();

            foreach (var nodo in nodos)
            {
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                    nodosnorton.Add(nodo);
            }

            TransientAnalysis analis = ana as TransientAnalysis;
            double w, wi, wf, deltaw, wx;

            wi = StringUtils.DecodeString(analis.Step);
            wf = StringUtils.DecodeString(analis.FinalTime);
            //wf = 1E5;//StringUtils.DecodeString(analis.EndFrequency);
            //w = wi;
            //int i = (int)Math.Log10(wi) + 1;
            //wx = Math.Pow(10, i);
            //if (analis.ScanType == ACAnalysis.ACAnalysisScan.Linear)
            //    deltaw = (wf - wi) / analis.Points;
            //else
            //    deltaw = (wx - wi) / analis.Points;


            //while (w < wf)
            //{
            //    var v = Vector<Complex32>.Build.Dense(nodosnorton.Count);
            //    var A = Matrix<Complex32>.Build.DenseOfArray(new Complex32[nodosnorton.Count, nodosnorton.Count]);

            //    foreach (var nodo in nodosnorton)
            //    {
            //        columna = fila = nodosnorton.IndexOf(nodo);
            //        Complex32 W = new Complex32(0,  (float)w);
            //        Complex32 Z, V;
            //        if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
            //            nodo.TypeOfNode == Node.NodeType.InternalBranchNode)
            //        {
            //            foreach (var rama in nodo.Components)
            //            {
            //                if (rama is Branch || rama is PasiveComponent)
            //                {
            //                    V = rama.NortonCurrent(nodo, W);
            //                    v[fila] += V;
            //                    Z = rama.Impedance(W).Reciprocal();
            //                    A[fila, columna] += Z;
            //                }
            //            }
            //        }
            //        else if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
            //        {
            //            Dipole compo1 = nodo.Components[0];
            //            Dipole compo2 = nodo.Components[1];
            //            Node nodo1 = compo1.OtherNode(nodo);
            //            Node nodo2 = compo2.OtherNode(nodo);
            //            Complex32 v1, v2, z1, z2;
            //            v1 = nodo1.Voltage;
            //            v2 = nodo2.Voltage;
            //            z1 = compo1.Impedance(W);
            //            z2 = compo2.Impedance(W);

            //            if (nodo1.IsVoltageKnowed)
            //                v[fila] += v1 * z1;
            //            else
            //            {
            //                columna = nodosnorton.IndexOf(nodo1);
            //                A[fila, columna] -= v1 * z1;
            //            }
            //            if (nodo2.IsVoltageKnowed)
            //                v[fila] += v2 * z2;
            //            else
            //            {
            //                columna = nodosnorton.IndexOf(nodo2);
            //                A[fila, columna] -= v2 * z2;
            //            }
            //            columna = nodosnorton.IndexOf(nodo);
            //            A[fila, columna] += z1 + z2;

            //            //nodo.Voltage = z2 * (v1 - v2) / (z1 + z2);

            //        }
            //        else if (nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode)
            //        {
            //            Dipole compo = nodo.Components[0];
            //            if (!(compo is VoltageGenerator))
            //                compo = nodo.Components[1];
            //            v[fila] = compo.Voltage.Real;
            //            A[fila, columna] = 1;
            //            columna = nodosnorton.IndexOf(compo.OtherNode(nodo));
            //            A[fila, columna] = -1;
            //        }
            //        else
            //            throw new NotImplementedException();
            //    }
            //    var x = A.Solve(v);

            //    Dictionary<string, Complex32> result = new Dictionary<string, Complex32>();
            //    foreach (var nodo in nodosnorton)
            //    {
            //        fila = nodosnorton.IndexOf(nodo);
            //        nodo.Voltage = x[fila];
            //        result.Add(nodo.Name, nodo.Voltage);
            //    }
            //    foreach (var nodo in nodos)
            //    {
            //        if (nodosnorton.Contains(nodo))
            //            continue;
            //        result.Add(nodo.Name, nodo.Voltage);
            //        //result.Add(
            //    }
            //    if (!Results.ContainsKey(w))
            //        Results.Add(w, result);

            //    if (analis.ScanType == ACAnalysis.ACAnalysisScan.Linear)
            //        w += deltaw;
            //    else
            //    {
            //        if (w > 0.95 * wx)
            //        {
            //            i++;
            //            wi = wx;
            //            wx = Math.Pow(10, i);
            //            deltaw = (wx - wi) / analis.Points;
            //        }
            //        //deltaw = (wx - wi);
            //        w = w * Math.Pow(10, deltaw/analis.Points);
            //    }
            //}

            return true;
        }

        
    }
}
