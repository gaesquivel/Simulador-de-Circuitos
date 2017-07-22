using ElectricalAnalysis.Analysis.Data;
using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class ParametricSolver:BasicSolver
    {
        
        //public new List<Tuple<BasicAnalysis, ParametricData>> Voltages { get; protected set; }
        //public new List<Tuple<BasicAnalysis, ParametricData>> Currents { get; protected set; }
        public new List<Tuple<BasicAnalysis, Complex, DataBase>> Voltages { get; protected set; }
        public new List<Tuple<BasicAnalysis, Complex, DataBase>> Currents { get; protected set; }

        /// <summary>
        /// Current name of parameter for the analisys
        /// </summary>
        public string ParameterName { get; set; }

        public ParametricSolver()
        {
            ParameterName = "";
            Voltages = new List<Tuple<BasicAnalysis, Complex, DataBase>>();
            Currents = new List<Tuple<BasicAnalysis, Complex, DataBase>>();
        }

        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            ParametricAnalisys param = ana as ParametricAnalisys;

            if (param == null)
            {
                return false;
            }

            Voltages.Clear();
            Currents.Clear();
            foreach (var anali in cir.Setup)
            {
                if (anali is ParametricAnalisys)
                    continue;

                //reemplazo el valor del parametro antes de resolver!
                for (int i = 0; i < param.Values.Count; i++)
                {
                    foreach (var item in cir.Components)
                    {
                        if (item.Name.ToLower() == param.Parameter && item is ElectricComponent)
                        {
                            ((ElectricComponent)item).Value = param.Values[i].Real;
                            break;
                        }
                    }
                    anali.Solver.Solve(cir, anali);
                    DataBase tensiones = null;
                    object corrientes = null;
                    if (anali is TransientAnalysis)
                    {
                        tensiones = ((TransientSolver)anali.Solver).Voltages;
                        corrientes = ((TransientSolver)anali.Solver).Currents;
                    }
                    else if (anali is ACAnalysis)
                    {
                        tensiones = ((ACSweepSolver)anali.Solver).Voltages;
                        corrientes = ((ACSweepSolver)anali.Solver).Currents;
                    }
                    else
                        continue;

                    //if (Voltages.ContainsKey(param.Values[i]))
                    //    return true;
                    Voltages.Add(new Tuple<BasicAnalysis, Complex, DataBase>(anali,
                                    param.Values[i], tensiones));
                    Currents.Add(new Tuple<BasicAnalysis, Complex, DataBase>(anali,
                                    param.Values[i], anali.Solver.Currents));
                    //break;
                }
            }

            return true;
        }
    }
}
