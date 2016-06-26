using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class TransientSolver:DCSolver
    {
        /// <summary>
        /// After solve, store a pair of values:
        ///             time             node voltage
        /// </summary>
        public Dictionary<double, Dictionary<string, double>> Voltages { get; protected set; }

        /// <summary>
        /// After solve, store a pair of values:
        ///             time            Component Current 
        /// </summary>
        public Dictionary<double, Dictionary<string, double>> Currents { get; protected set; }


        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            cir.State = Circuit.CircuitState.Solving;

            SolveInfo solveinfo = PreAnalizeToSolve(cir);
            List<NodeSingle> nodos = new List<NodeSingle>();
            Voltages = new Dictionary<double, Dictionary<string, double>>();
            Currents = new Dictionary<double, Dictionary<string, double>>();

            TransientAnalysis analis = ana as TransientAnalysis;
            double t, tf, deltat;
            
            #region time initialization

            deltat = StringUtils.DecodeString(analis.Step);
            tf = StringUtils.DecodeString(analis.FinalTime);
            t = 0;
            cir.CircuitTime = t;
            cir.Reset();

            #endregion
            //Progress = 0;
            //await Task.Run(async () =>
            //{
            while (t < tf)
            {
                CurrentCircuit.CircuitTime = t;
                //Calculo de tensiones de nodos
                //await Task.Run(()=> Calculate(solveinfo, t));
                Calculate(solveinfo, t);

                Dictionary<string, double> result = new Dictionary<string, double>();

                #region almacenamiento temporal

                foreach (var nodo in cir.Nodes.Values)
                {
                    if (double.IsNaN(nodo.Voltage.Real))
                    {
                        NotificationsVM.Instance.Notifications.Add(
                            new Notification("An invalid value was calculated at simulation at time " + t.ToString(), 
                                                Notification.ErrorType.error));
                        cir.State = Circuit.CircuitState.Solved;
                        return false;
                    }
                    result.Add(nodo.Name, nodo.Voltage.Real);
                }

                if (!Voltages.ContainsKey(t))
                    Voltages.Add(t, result);

                #endregion

                //calculo las corrientes:
                CalculateCurrents(cir, t);
                Dictionary<string, double> currents = new Dictionary<string, double>();
                StorageCurrents(CurrentCircuit, currents);
                Currents.Add(t, currents);


                cir.CircuitTime = t;
                t += deltat;
                // Progress = (int)(100 * t / tf);
            }
            cir.State = Circuit.CircuitState.Solved;
            //}).ConfigureAwait(true);
            return true;
        }

        private static void StorageCurrents(ComponentContainer cir, Dictionary<string, double> currents)
        {
            foreach (var compo in cir.Components)
            {
                currents.Add(compo.Name, compo.current.Real);
                if (compo is ComponentContainer)
                {
                    StorageCurrents((ComponentContainer)compo, currents);
                }
            }
        }

        protected override bool IsOpenCircuit(Dipole comp)
        {
            return false;
        }
        protected override bool IsShortCircuit(Dipole comp)
        {
            return false;
        }

        protected override bool IsPartOfSuperNode(Dipole compo)
        {
            if (compo is VoltageGenerator || compo is Capacitor)
                return true;
            return false;
        }

        protected override Complex GetVoltage(Dipole V, NodeSingle nodo, object e)
        {
            if (nodo == null)
                throw new NullReferenceException();
            if (e is double)
                return V.voltage(nodo, (double)e);
            throw new NotImplementedException();
        }

        protected override bool IsCurrentGenerator(Dipole comp)
        {
            if (comp is Inductor || comp is CurrentGenerator)
                return true;
            return false; //base.IsCurrentGenerator(comp);
        }

        protected override bool IsVoltageGenerator(Dipole comp)
        {
            if (comp is Capacitor || comp is VoltageGenerator)
                return true;
            return false;//base.IsVoltageGenerator(comp);   
        }

        public override bool Export(string FileName, ExportFormats format = ExportFormats.CSV)
        {
            switch (format)
            {
                case ExportFormats.CSV:
                    //se guarda@)
                    //t     N1  N2 .... Nn      cmp1    cmp2    ....
                    //10    5   15      2       1mA     2mA
                    //12    6   14      1       0,5mA   3mA
                    //List<List<string>> todos = new List<List<string>>();
                    using (var writer = new CsvFileWriter(FileName))
                    {
                        writer.Delimiter = ';';
                        List<string> results = new List<string>();
                        results.Add("t");
                        //List<string> componames = new List<string>();
                        //results.Clear();
                        foreach (var item in Voltages)
                        {
                            foreach (var node in item.Value)
                                results.Add(node.Key.ToString());
                            break;
                        }
                        foreach (var compo in Currents.Values)
                        {
                            foreach (var componame in compo.Keys)
                                results.Add(componame);
                            break;
                        }
                        writer.WriteRow(results);
                        // results.Clear();


                        foreach (var item in Voltages)
                        {
                            results.Clear();
                            // tiempo
                            results.Add(item.Key.ToString());
                            //los nodos
                            foreach (var node in item.Value)
                            {
                                results.Add(node.Value.ToString());
                            }
                            //corrientes
                            Dictionary<string, double> currs = Currents[item.Key];
                            foreach (var corr in currs.Values)
                            {
                                results.Add(corr.ToString());
                            }
                            writer.WriteRow(results);
                        }
                    }
                    return true;
                case ExportFormats.Bitmap:
                    throw new NotImplementedException();
                    break;
                default:
                    break;
            }
            return false;
        }

    }
}
