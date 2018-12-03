using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis.Components;
using MathNet.Numerics.LinearAlgebra;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;
using ElectricalAnalysis.Components.Controlled;
using ElectricalAnalysis.Analysis.Data;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class ACSweepSolver : DCSolver
    {


        /// <summary>
        /// After run Solver, return a list of pairs of values:
        ///         W                NodeName    NodeVoltage
        /// </summary>
        public new FrequencyData Voltages { get; protected set; }
        //public virtual Dictionary<double, Dictionary<string, Complex>> Voltages { get; protected set; }

        /// <summary>
        /// After run Solvermethod, return a list a pairs of values:
        ///         W              CompoName   CompoCurrent
        /// </summary>
        public new FrequencyData Currents { get; protected set; }
        //public virtual Dictionary<double, Dictionary<string, Complex>> Currents { get; protected set; }


        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            cir.State = Circuit.CircuitState.Solving;

            Voltages = new FrequencyData();//Dictionary<double, Dictionary<string, Complex>>();
            Currents = new FrequencyData();//Dictionary<double, Dictionary<string, Complex>>();

            SolveInfo solveinfo = PreAnalizeToSolve(cir);

            ACAnalysis analis = ana as ACAnalysis;
            double w, wi, wf, deltaw, wx, pow = 1;

            #region W initialization

            if (!StringUtils.DecodeString(analis.StartFrequency, out wi))
            {
                NotificationsVM.Instance.Notifications.Add(new Notification("Error reading Wi", Notification.ErrorType.error));
                return false;
            }
            if (!StringUtils.DecodeString(analis.EndFrequency, out wf))
            {
                NotificationsVM.Instance.Notifications.Add(new Notification("Error reading Wf", Notification.ErrorType.error));
                return false;
            }
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

            #endregion

            while (w < wf)
            {
                //Calculo de tensiones de nodos
                Complex W = new Complex(0, w);
                //cir.Reset();
                Calculate(solveinfo, W);

                Dictionary<string, Complex> result = new Dictionary<string, Complex>();

                #region almacenamiento temporal

                foreach (var nodo in cir.Nodes.Values)
                {
                    result.Add(nodo.Name, nodo.Voltage);
                }

                if (!Voltages.ContainsKey(w))
                    Voltages.Add(w, result);

                #endregion

                //calculo las corrientes:
                CalculateCurrents(cir, W);
                Dictionary<string, Complex> currents = new Dictionary<string, Complex>();
                StorageCurrents(cir, currents);
                Currents.Add(w, currents);

                #region w actualization

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

                #endregion  
            }

            cir.State = Circuit.CircuitState.Solved;
            return true;
        }

        /// <summary>
        /// Almacena las corrientes del W actual
        /// </summary>
        /// <param name="cir"></param>
        /// <param name="currents"></param>
        protected void StorageCurrents(ComponentContainer cir, Dictionary<string, Complex> currents)
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

        protected override bool IsPartOfSuperNode(Dipole compo)
        {
            return compo is VoltageGenerator && !(compo is Components.Controlled.ControlledDipole);
        }


        protected override Complex GetVoltage(Dipole V, NodeSingle nodo, object e)
        {
            if (nodo == null)
                throw new NullReferenceException();
            if (e is Complex)
                return V.voltage(nodo, e as Complex?);
            throw new NotImplementedException();
        }

        protected override bool IsImpedance(Dipole comp)
        {
            if (comp is Capacitor || comp is Inductor || comp is Resistor)
                return true;
            return false;
        }

        protected override bool IsVoltageGenerator(Dipole comp)
        {
            return comp is VoltageGenerator && !(comp is ControlledDipole);
        }

        public override bool Export(string FileName, ExportFormats format = ExportFormats.CSV)
        {
            //se guarda@)
            //W     N1  N2 .... Nn
            //10    5   15      2
            //12    6   14      1

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


                foreach (var item in Voltages)
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
            return true;
        }
    }
}