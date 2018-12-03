using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class ComplexPlainSolver:ACSweepSolver
    {

        /// <summary>
        /// Storage W and their indexes
        ///                     i   j       W 
        /// </summary>
        public Dictionary<Tuple<int, int>, Complex> WfromIndexes { get; protected set; }  

        /// <summary>
        /// Storage the different values of W and a dictionary with the nodes and their Voltages
        /// </summary>
        public new Dictionary<Complex, Dictionary<string, Complex>> Voltages { get; protected set; }//W  Nname    Nvoltage

        /// <summary>
        /// Store values of W and currents of each component
        ///                  W              CompoName   CompoCurrent
        /// </summary>
        public new Dictionary<Complex, Dictionary<string, Complex>> Currents { get; protected set; }
        public NodeSingle SelectedNode { get; private set; }

        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            cir.State = Circuit.CircuitState.Solving;

            NotificationsVM.Instance.Notifications.Add(new Notification("Solving..."));
            Voltages = new Dictionary<Complex, Dictionary<string, Complex>>();
            Currents = new Dictionary<Complex, Dictionary<string, Complex>>();
            WfromIndexes = new Dictionary<Tuple<int, int>, Complex>();
            
            SolveInfo solveinfo = PreAnalizeToSolve(cir);


            ComplexPlainAnalysis analis = ana as ComplexPlainAnalysis;
            double w, sig = 0, wi = 0, wf = 0, deltaw = 0, deltasig = 0, sigmamin = 0, sigmamax = 0;

            #region W initialization

            double val = 0;
            if (StringUtils.DecodeString(analis.WMin, out val))
                wi = val;
            if (StringUtils.DecodeString(analis.WMax, out val))
                wf = val;
            if (StringUtils.DecodeString(analis.SigmaMax, out val))
                sigmamax = val;
            if (StringUtils.DecodeString(analis.SigmaMin, out val))
                sigmamin = val;
            w = wi;
            sig = sigmamin;
            deltaw = (wf - wi) / (analis.Points -1);
            deltasig = (sigmamax - sigmamin) / (analis.Points -1);

            Complex S = Complex.Zero;

            #endregion

            for (int i = 0; i < analis.Points; i++)
            {
                w = (wi + i * deltaw);
                for (int j = 0; j < analis.Points; j++)
                {
                    double sigma = (sigmamin + j * deltasig);
                    if (w == 0 && sigma == 0)
                        NotificationsVM.Instance.Notifications.Add(
                                new Notification("w = Cero: i=" + i.ToString() + ", j=" + j.ToString()));
                    //Calculo de tensiones de nodos
                    S = new Complex(sigma, w);
                    Calculate(solveinfo, S);

                    //          Node    Voltage
                    Dictionary<string, Complex> result = new Dictionary<string, Complex>();

                    #region almacenamiento temporal

                    foreach (var nodo in cir.Nodes.Values)
                    {
                        result.Add(nodo.Name, nodo.Voltage);
                    }

                    if (!Voltages.ContainsKey(S))
                    {
                        Voltages.Add(S, result);
                        WfromIndexes.Add(new Tuple<int, int>(i, j), S);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    #endregion

                    //calculo las corrientes:
                    CalculateCurrents(cir, S);
                    Dictionary<string, Complex> currents = new Dictionary<string, Complex>();
                    StorageCurrents(cir, currents);
                    Currents.Add(S, currents);
                }
            }

            cir.State = Circuit.CircuitState.Solved;
            return true;
        }

        public override bool Export(string FileName, ExportFormats format = ExportFormats.CSV)
        {
            if (SelectedNode == null)
            {
                if(CurrentCircuit.Nodes.ContainsKey("out"))
                    SelectedNode = CurrentCircuit.Nodes["out"];
                else
                {
                    NotificationsVM.Instance.Notifications.Add(
                        new Notification("No node or component selected. must select one!", 
                                        Notification.ErrorType.error));
                    return false;
                }
            }

            ComplexPlainAnalysis CurrentAnalysis = null;
            foreach (var analis in this.CurrentCircuit.Setup)
            {
                if (analis is ComplexPlainAnalysis)
                {
                    CurrentAnalysis = analis as ComplexPlainAnalysis;
                    break;
                }
            }
            using (var writer = new CsvFileWriter(FileName))
            {
                writer.Delimiter = ';';
                List<string> results = new List<string>();

                Complex W;
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
            return true;
        }


        public System.Drawing.Bitmap TakeSnapShot(NodeSingle node, ComplexPlainAnalysis CurrentAnalysis)
        {
            SelectedNode = node;
            ComplexPlainAnalysis SnapAnalysis = (ComplexPlainAnalysis)CurrentAnalysis;//.Clone();
            //SnapAnalysis.Points = 300;
            //Solve(CurrentCircuit, SnapAnalysis);

            System.Drawing.Bitmap bmp = FileUtils.DrawImage((x, y) =>
                {
                    Complex W = WfromIndexes[new Tuple<int, int>(x, y)];
                    foreach (var nodo in Voltages[W])
                    {
                        if (nodo.Key == SelectedNode.Name)
                        {
                            return nodo.Value;
                        }
                    }
                    return Complex.Zero;
                }
                , SnapAnalysis.Points, SnapAnalysis.Points);

            return bmp;
        }

        //private Complex func(int x, int y)
        //{
        //    Complex W = WfromIndexes[new Tuple<int, int>(x, y)];
        //    foreach (var node in Voltages[W])
        //    {
        //        if (node.Key == SelectedNode.Name)
        //        {
        //            return node.Value;
        //        }
        //    }
        //    return Complex.Zero;
        //}

    }
}
