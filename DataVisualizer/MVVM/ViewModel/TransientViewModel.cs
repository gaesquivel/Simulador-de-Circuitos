using CircuitMVVMBase.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace DataVisualizer.MVVM.ViewModel
{
    public class TransientViewModel: Plotter2DViewModel
    {

        ObservableDataSource<Tuple<double, double>> DataSource = null;
        public ChartPlotter Plotter { get; set; }
        public LineGraph linegraph { get; set; }

        public TransientViewModel()
        {
            DataSource = new ObservableDataSource<Tuple<double, double>>();
            Name = "Transient";
            Initialize();
        }

        private void Initialize()
        {
            // Create first source
            DataSource = new ObservableDataSource<Tuple<double, double>>();
            if (Plotter == null)
                return;

            DataSource.SetXYMapping(z => {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });
            HorizontalAxis axis = (HorizontalAxis)Plotter.MainHorizontalAxis;
            axis.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            VerticalAxis axis2 = (VerticalAxis)Plotter.MainVerticalAxis;
            axis2.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

           

            linegraph.DataSource = DataSource;
        }

        protected override bool IsAnalisysType(BasicAnalysis analis)
        {
            return analis is TransientAnalysis;
        }

        protected override void Redraw(object obj)
        {
            TransientAnalysis analis = CurrentAnalisys() as TransientAnalysis;
            TransientSolver sol5 = analis.Solver as TransientSolver;
            DataSource.Collection.Clear();

            if (SelectedObject is Node)
            {
                AddVoltage(sol5, ((Node)SelectedObject).Name);
            }
            else if (SelectedObject is Dipole)
            {
                AddCurrent(sol5, ((Dipole)SelectedObject).Name);
            }
            else if (CurrentCircuit != null)
            {
                if (CurrentCircuit.Nodes.ContainsKey("out"))
                    AddVoltage(sol5, "out");
            }
        }

        public override void Simulate(object obj)
        {
            IsBusy = true;
            string file = "";
            TransientAnalysis ac1 = null;
            if (CurrentCircuit == null || obj is string)
            {
                CurrentCircuit = new Circuit();
                //CurrentCircuit.Setup.RemoveAt(0);
            }
            ac1 = CurrentAnalisys() as TransientAnalysis;
            if (ac1 == null)
            {
                foreach (var item in MainObjects)
                {
                    if (item is TransientAnalysis)
                    {
                        ac1 = item as TransientAnalysis;
                        break;
                    }
                }
                if (ac1 == null)
                    ac1 = new TransientAnalysis();
                CurrentCircuit.Setup.Add(ac1);
            }
            if (!MainObjects.Contains(ac1))
                MainObjects.Add(ac1);

            if (linegraph.DataSource == null)
            {
                Initialize();
            }

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


        private void AddVoltage(TransientSolver sol5, string name)
        {
            foreach (var data in sol5.Voltages)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == name)
                    {
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value);
                        DataSource.Collection.Add(p);
                    }
                }
            }
        }

        private void AddCurrent(TransientSolver sol5, string name)
        {
            foreach (var data in sol5.Currents)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == name)
                    {
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value);
                        DataSource.Collection.Add(p);
                    }
                }
            }
        }

    }
}
