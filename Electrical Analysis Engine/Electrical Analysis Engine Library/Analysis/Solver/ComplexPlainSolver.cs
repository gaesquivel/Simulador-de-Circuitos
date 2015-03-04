using ElectricalAnalysis.Components;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class ComplexPlainSolver:ACSweepSolver
    {

        public Node SelectedNode { get; set; }
        public ComplexPlainAnalysis CurrentAnalysis { get; set; }
        public Circuit CurrentCircuit { get; set; }

        /// <summary>
        /// Storage W and their indexes
        /// </summary>
        public Dictionary<Tuple<int, int>, Complex32> WfromIndexes { get; set; } // i   j  W  
        
        /// <summary>
        /// Storage the different values of W and a dictionary with the nodes and their Voltages
        /// </summary>
        public new Dictionary<Complex32, Dictionary<string, Complex32>> Voltages { get; protected set; }//W  Nname    Nvoltage
  
        //                  W              CompoName   CompoCurrent
        public new Dictionary<Complex32, Dictionary<string, Complex32>> Currents { get; protected set; }

        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            //int fila = 0;
            List<Node> nodos = new List<Node>();
            CurrentAnalysis = ana as ComplexPlainAnalysis;
            CurrentCircuit = cir;


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

            ComplexPlainAnalysis analis = ana as ComplexPlainAnalysis;
            double w, sig, wi, wf, deltaw, deltasig, sigmamin, sigmamax;

            wi = analis.WMin;
            wf = analis.WMax;
            sigmamax = analis.SigmaMax;
            sigmamin = analis.SigmaMin;
            w = wi;
            sig = sigmamin;
            deltaw = (wf - wi) / analis.Points;
            deltasig = (sigmamax - sigmamin) / analis.Points;

            Voltages = new Dictionary<Complex32, Dictionary<string, Complex32>>();
            Currents = new Dictionary<Complex32, Dictionary<string, Complex32>>();
            WfromIndexes = new Dictionary<Tuple<int, int>, Complex32>();
            Complex32 W = Complex32.Zero;

            for (int i = 0; i <= analis.Points; i++)
            {

                for (int j = 0; j <= analis.Points; j++)
                {
                    nortoncopia.Clear();
                    foreach (var item in nodosnorton)
                    {
                        nortoncopia.Add(item);
                    }

                    //Calculo de tensiones de nodos
                    W = new Complex32((float)(sigmamin + j * deltasig), (float)(wi + i * deltaw));
                    ACSweepSolver.Calculate(nortoncopia, W);
                    
                    //          Node    Voltage
                    Dictionary<string, Complex32> result = new Dictionary<string, Complex32>();

                    #region almacenamiento temporal
                  
                    foreach (var nodo in nodos)
                    {
                        result.Add(nodo.Name, nodo.Voltage);
                    }
                    if (!Voltages.ContainsKey(W))
                    {
                        Voltages.Add(W, result);
                        WfromIndexes.Add(new Tuple<int, int>(i, j), W);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                   // Results[i,j] = new Tuple<Complex32,Complex32>(W, 

                    #endregion

                    //calculo las corrientes:
                    CalculateCurrents(cir, W);
                    Dictionary<string, Complex32> currents = new Dictionary<string, Complex32>();
                    StorageCurrents(cir, currents);
                    Currents.Add(W, currents);

                }
            }

            return true;
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


        public override void ExportToCSV(string FileName)
        {
            if (SelectedNode == null)
            {
                Node[] arr = CurrentCircuit.Nodes.Values.ToArray<Node>();
                SelectedNode = arr[0];
            }

            using (var writer = new CsvFileWriter(FileName))
            {
                writer.Delimiter = ';';
                List<string> results = new List<string>();

                MathNet.Numerics.Complex32 W;
                for (int i = 0; i < CurrentAnalysis.Points; i++)
                {
                    for (int j = 0; j < CurrentAnalysis.Points; j++)
                    {
                        W = WfromIndexes[new Tuple<int, int>(i, j)];
                        foreach (var node in Voltages[W])
                        {
                            if (node.Key == SelectedNode.Name)
                            { //W.Real ,
                                //                        W.Imaginary 
                                results.Add(node.Value.Magnitude.ToString());
                                break;
                            //                      2 * Math.Log10(node.Value.Magnitude));
                            }
                        }
                    }
                    writer.WriteRow(results);
                    results.Clear();

                }
            }
        }

    }
}
