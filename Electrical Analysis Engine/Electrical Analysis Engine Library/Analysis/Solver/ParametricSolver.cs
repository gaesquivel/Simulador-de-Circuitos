using ElectricalAnalysis.Analysis.Data;
using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class ParametricSolver:BasicSolver
    {
        public class Parameter
        {
            public string Name { get; set; }
            public Complex Value { get; set; }
        }


        //public new List<Tuple<BasicAnalysis, ParametricData>> Voltages { get; protected set; }
        //public new List<Tuple<BasicAnalysis, ParametricData>> Currents { get; protected set; }
        public new List<Tuple<BasicAnalysis, Parameter, DataBase>> Voltages { get; protected set; }
        public new List<Tuple<BasicAnalysis, Parameter, DataBase>> Currents { get; protected set; }

        /// <summary>
        /// Current name of parameter for the analisys
        /// </summary>
        public string ParameterName { get; set; }

        public ParametricSolver()
        {
            ParameterName = "";
            Voltages = new List<Tuple<BasicAnalysis, Parameter, DataBase>>();
            Currents = new List<Tuple<BasicAnalysis, Parameter, DataBase>>();
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
                {
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
                        SolveBase(cir, anali, new Parameter() {Name = ParameterName, Value = param.Values[i]});
                    }
                }
                else
                {
                    SolveBase(cir, anali, null);
                }
            }

            return true;
        }

        private void SolveBase(Circuit cir, BasicAnalysis anali, Parameter paramvalue)
        {
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
                return;

            //if (Voltages.ContainsKey(param.Values[i]))
            //    return true;
            //Complex value = paramvalue.Value;
            Voltages.Add(new Tuple<BasicAnalysis, Parameter, DataBase>(anali,
                            paramvalue, tensiones));
            Currents.Add(new Tuple<BasicAnalysis, Parameter, DataBase>(anali,
                            paramvalue, anali.Solver.Currents));
        }
    }
}
