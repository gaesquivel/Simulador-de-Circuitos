using CircuitMVVMBase.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexPlainVisualizer.MVVM.ViewModel
{
    public class DCViewModel : CircuitSimulationViewModel
    {

        public DCViewModel()
        {
            Name = "DC Bias Point";

        }

        public override void Redraw(object obj)
        {
            DCAnalysis analis = CurrentAnalisys() as DCAnalysis;
            DCSolver sol5 = analis.Solver as DCSolver;
            //DataSource.Collection.Clear();

            if (SelectedObject is Node)
            {
                //AddVoltage(sol5, ((Node)SelectedObject).Name);
            }
            else if (SelectedObject is Dipole)
            {
                //AddCurrent(sol5, ((Dipole)SelectedObject).Name);
            }
            else if (CurrentCircuit != null)
            {
                if (CurrentCircuit.Nodes.ContainsKey("out"))
                { }// AddVoltage(sol5, "out");
            }
        }

        public override void Simulate(object obj)
        {
            IsBusy = true;
            string file = "";
            DCAnalysis ac1 = null;
            if (CurrentCircuit == null || obj is string)
            {
                CurrentCircuit = new Circuit();
                //CurrentCircuit.Setup.RemoveAt(0);
            }
            ac1 = CurrentAnalisys() as DCAnalysis;
            if (ac1 == null)
            {
                foreach (var item in MainObjects)
                {
                    if (item is DCAnalysis)
                    {
                        ac1 = item as DCAnalysis;
                        break;
                    }
                }
                if (ac1 == null)
                    ac1 = new DCAnalysis();
                CurrentCircuit.Setup.Add(ac1);
            }
            if (!MainObjects.Contains(ac1))
                MainObjects.Add(ac1);

            //if (linegraph.DataSource == null)
            //{
            //    Initialize();
            //}

            if (obj is string)
            {
                file = obj as string;
                CurrentCircuit.ReadCircuit(file);
            }
            //await Task.Run(() => CurrentCircuit.Solve(ac1));
            IsBusy = CurrentCircuit.Solve(ac1);


            Redraw(false);
            //IsBusy = false;
        }

        public override void ShowPlot(object obj)
        {
            throw new NotImplementedException();
        }

        public override void StoragePlot(object obj)
        {
            throw new NotImplementedException();
        }

        public override void DeletePlot(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
