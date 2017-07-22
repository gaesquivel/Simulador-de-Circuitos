using CircuitMVVMBase.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Data;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DataVisualizer.MVVM.ViewModel
{
    public class TransientViewModel: CircuitSimulationViewModel
    {

        [Browsable(false)]
        public ObservableDataSource<Tuple<double, double>> DataSource { get; set; } = null;
        [Browsable(false)]
        public ChartPlotter Plotter { get; set; }
        [Browsable(false)]
        public LineGraph linegraph { get; set; }

        public TransientViewModel()
        {
            //DataSource = new ObservableDataSource<Tuple<double, double>>();
            Name = "Transient";
            // Create first source
            DataSource = new ObservableDataSource<Tuple<double, double>>();
            if (Plotter == null)
                return;

            //Initialize();
        }

        private void Initialize(LineGraph linegraph, ObservableDataSource<Tuple<double, double>> datasource)
        {

            datasource.SetXYMapping(z => {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });
            HorizontalAxis axis = (HorizontalAxis)Plotter.MainHorizontalAxis;
            axis.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            VerticalAxis axis2 = (VerticalAxis)Plotter.MainVerticalAxis;
            axis2.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            linegraph.DataSource = datasource;
        }

        #region Plot Storage

        public override void StoragePlot(object obj)
        {
            if (linegraph != null )
                foreach (var item in PlottedItems)
                {
                    //si ya esta no lo almaceno
                    if (linegraph.Tag == item.Item2.Tag)
                    {
                        return;
                    }
                }

            //if (!PlottedItems.Contains(linegraph))
            {
                //crea una copia y la guarda
                LineGraph line = new LineGraph();
                line.Name = "Tran" + PlottedItems.Count.ToString();
                line.Tag = linegraph.Tag;

                int n = DataSource.Collection.Count;
                Tuple<double, double>[] arr = new Tuple<double, double>[n];
                DataSource.Collection.CopyTo(arr, 0);
                var col = new ObservableDataSource<Tuple<double, double>>(arr);
                col.SetXYMapping(z =>
                {
                    Point p = new Point(z.Item1, z.Item2);
                    return p;
                });

                line.DataSource = col;
                PlottedItems.Add(new Tuple<Plotter2D, LineGraph>(Plotter, line));
            }
        }

        public override void ClearPlots(object obj)
        {
            foreach (var item in PlottedItems)
            {
                LineGraph line = item.Item2;
                if (Plotter.Children.Contains(line))
                    Plotter.Children.Remove(line);
            }
            PlottedItems.Clear();
            SelectedPlot = null;
        }

        public override void ShowPlot(object obj)
        {
            if (PlottedItems.Count > 0 &&
                SelectedPlot != null &&
                !Plotter.Children.Contains(SelectedPlot))
                Plotter.Children.Add(SelectedPlot);
        }


        public override void DeletePlot(object obj)
        {
            if (PlottedItems.Count > 0 && SelectedPlot != null)
            {
                foreach (var item in PlottedItems)
                {
                    if (SelectedPlot == item.Item2)
                    {
                        PlottedItems.Remove(item);
                        break;
                    }
                }
                Plotter.Children.Remove(SelectedPlot);
                SelectedPlot = null;
            }
        }
        #endregion

        protected override void OnShowTrack()
        {
            if(ShowTrack && !Plotter.Children.Contains(mouseTrack))
                Plotter.Children.Add(mouseTrack);
            else if (Plotter.Children.Contains(mouseTrack))
                Plotter.Children.Remove(mouseTrack);
        }

        protected override bool IsAnalisysType(BasicAnalysis analis)
        {
            return analis is TransientAnalysis;
        }

        public override void Redraw(object obj)
        {
            if (obj is Tuple<BasicAnalysis, Complex, DataBase>)
            {
                //para analisis parametrico
                string name = "out";

                if (SelectedObject is Node)
                {
                    name = ((Node)SelectedObject).Name;
                }
                var voltages = ((Tuple<BasicAnalysis, Complex, DataBase>)obj).Item3 as TransientData;
                var param = ((Tuple<BasicAnalysis, Complex, DataBase>)obj).Item2;
                PlottedItems.Clear();
                ShowVoltage(voltages, name, true);
                Legend.SetDescription(linegraph, name + " " + param.Real.ToString());
                return;
            }

            //analisis convencional
            TransientAnalysis analis = CurrentAnalisys() as TransientAnalysis;
            TransientSolver sol5 = analis.Solver as TransientSolver;
            DataSource.Collection.Clear();

            if (SelectedObject is Node)
            {
                string name = ((Node)SelectedObject).Name;
                if (ShowVoltage(sol5.Voltages, name))
                    PlottedItem = sol5.CurrentCircuit.Nodes[name] as Item;
            }
            else if (SelectedObject is Dipole)
            {
                ShowCurrent(sol5, ((Dipole)SelectedObject).Name);
            }
            else if (CurrentCircuit != null)
            {
                if (CurrentCircuit.Nodes.ContainsKey("out"))
                {
                    ShowVoltage(sol5.Voltages, "out");
                    PlottedItem = sol5.CurrentCircuit.Nodes["out"] as Item;
                }
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
            if (CurrentCircuit.IsChanged)
            {
                CurrentCircuit.Parse();
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
                Initialize(linegraph, DataSource);
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


        int alfa = 255;
        //public static double DecrementAlfa(double Original)
        //{

        //}

        protected bool ShowVoltage(TransientData voltages, string name, bool add = false)
        {
            bool wasfinded = false;
            if (add)
            {
                DataSource = new ObservableDataSource<Tuple<double, double>>();
                linegraph = new LineGraph(DataSource);
                linegraph.Stroke = BrushHelper.MakeTransparent(Brushes.Blue, alfa-=30);
                linegraph.StrokeThickness = 2;
                Initialize(linegraph, DataSource);

                Plotter.Children.Add(linegraph);
                PlottedItems.Add(new Tuple<Plotter2D, LineGraph>(Plotter, linegraph));
            }
            foreach (var data in voltages)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == name)
                    {
                        wasfinded = true;
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value);
                        DataSource.Collection.Add(p);
                    }
                }
            }
           
            return wasfinded;
        }

        protected void ShowCurrent(TransientSolver solver, string name)
        {
            bool wasfinded = false;
            foreach (var data in solver.Currents)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == name)
                    {
                        wasfinded = true;
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value);
                        DataSource.Collection.Add(p);
                    }
                }

            }
            if (wasfinded)
                foreach (var item in solver.CurrentCircuit.Components)
                {
                    if (item.Name == name)
                    {
                        PlottedItem = item;
                        break;
                    }
                }
            //    PlottedItem = sol5.CurrentCircuit.Components[name] as Item;
        }

    }
}
