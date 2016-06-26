using CircuitMVVMBase.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace DataVisualizer.MVVM.ViewModel
{
    public class BodeViewModel: Plotter2DViewModel
    {

        ObservableDataSource<Tuple<double, double>> source1 = null;
        ObservableDataSource<Tuple<double, double>> source2 = null;

        public ObservableDataSource<Tuple<double, double>> ModuleData
        {
            get { return source1; }
            protected set { RaisePropertyChanged(value, ref source1); }
        }

        public ObservableDataSource<Tuple<double, double>> PhaseData
        {
            get { return source2; }
            protected set { RaisePropertyChanged(value, ref source2); }
        }

        public ChartPlotter ModulePlotter { get;  set; }
        public ChartPlotter PhasePlotter { get; set; }
        public LineGraph linegraph { get;  set; }
        public LineGraph phasegraph { get; set; }

        public BodeViewModel()
        {
            Name = "Bode/AC sweep";
            source1 = new ObservableDataSource<Tuple<double, double>>();
            source2 = new ObservableDataSource<Tuple<double, double>>();

            source1.SetXYMapping(z =>
            {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });

            source2.SetXYMapping(z =>
            {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });
            MainObjects.Add(this);
            Initialize();
        }

        private void Initialize()
        {
            if (ModulePlotter == null)
                return;

            ModulePlotter.DataTransform = new Log10Transform();

            ModulePlotter.MainHorizontalAxis = new HorizontalAxis
            {
                TicksProvider = new LogarithmNumericTicksProvider(10),
                LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => 
                                                StringUtils.CodeString(info.Tick) }
            };

            ModulePlotter.MainVerticalAxis = new VerticalAxis
            {
                TicksProvider = new LogarithmNumericTicksProvider(10),
                LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => 
                                                StringUtils.CodeString(info.Tick) }
            };


            PhasePlotter.DataTransform = new Log10XTransform();
            PhasePlotter.MainHorizontalAxis = new HorizontalAxis
            {
                TicksProvider = new LogarithmNumericTicksProvider(10),
                LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => 
                                                StringUtils.CodeString(info.Tick) }
            };
            PhasePlotter.MainVerticalAxis = new VerticalAxis
            {
                TicksProvider = new CustomBaseNumericTicksProvider(15),
    //            LabelProvider = new UnroundingLabelProvider()
    //            {
    //                CustomFormatter = info =>
    //StringUtils.CodeString(info.Tick)
    //            }
            };


            linegraph.DataSource = source1;
            phasegraph.DataSource = source2;

        }

        protected override void StoragePlot(object obj)
        {
            if (linegraph != null && !PlottedItems.Contains(linegraph))
            {
                LineGraph line = new LineGraph();
                line.Name = "Mod" + PlottedItems.Count.ToString();
                //line.
                int n = source1.Collection.Count;
                Tuple<double, double>[] arr = new Tuple<double, double>[n];
                source1.Collection.CopyTo(arr, 0);
                var col= new ObservableDataSource<Tuple<double, double>>(arr);
                col.SetXYMapping(z =>
                {
                    Point p = new Point(z.Item1, z.Item2);
                    return p;
                });

                line.DataSource = col;
                PlottedItems.Add(line);
            }
        }

        protected override void ClearPlots(object obj)
        {
            foreach (var item in PlottedItems)
            {
                if (ModulePlotter.Children.Contains(item))
                    ModulePlotter.Children.Remove(item);
            }
            PlottedItems.Clear();
            SelectedPlot = null;
        }

        protected override void ShowPlot(object obj)
        {
            if (PlottedItems.Count > 0 &&
                SelectedPlot != null &&
                !ModulePlotter.Children.Contains(SelectedPlot))
                    ModulePlotter.Children.Add(SelectedPlot);
        }

        protected override void DeletePlot(object obj)
        {
            if (PlottedItems.Count > 0 && SelectedPlot != null)
            {
                PlottedItems.Remove(SelectedPlot);
                ModulePlotter.Children.Remove(SelectedPlot);
                SelectedPlot = null;
            }
        }

        protected override bool IsAnalisysType(BasicAnalysis analis)
        {
            return analis is ACAnalysis;
        }

        protected override void Redraw(object obj)
        {
            //ACSweepSolver sol5 = (ACSweepSolver)CurrentCircuit.Setup[0].Solver;
            ACAnalysis analis = CurrentAnalisys() as ACAnalysis;
            ACSweepSolver sol5 = analis.Solver as ACSweepSolver;
            //(ACSweepSolver) from sol in CurrentCircuit.Setup
            //                 where sol is ACAnalysis select sol.Solver;
            source1.Collection.Clear();
            source2.Collection.Clear();

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
            string file = "";
            ACAnalysis ac1;
            if (CurrentCircuit == null || obj is string)
            {
                CurrentCircuit = new Circuit();
                //ac1 = (ACAnalysis)from sol in CurrentCircuit.Setup
                //                  where sol is ACAnalysis
                //                  select sol;
              
            }
            ac1 = CurrentAnalisys() as ACAnalysis;
            if (ac1 == null)
            {
                ac1 = new ACAnalysis();
                CurrentCircuit.Setup.Add(ac1);
                if (!MainObjects.Contains(ac1))
                    MainObjects.Add(ac1);
            }
            if (obj is string)
            {
                file = obj as string;
                CurrentCircuit.ReadCircuit(file);
            }
            if (CurrentCircuit.IsChanged)
                if (!CurrentCircuit.Parse())
                    return;
            CurrentCircuit.Solve(ac1);

            Redraw(false);

            if (linegraph.DataSource == null)
            {
                Initialize();
            }
        }



        private void AddVoltage(ACSweepSolver sol5, string NodeName)
        {
            foreach (var data in sol5.Voltages)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == NodeName)
                    {
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value.Magnitude);
                        ModuleData.Collection.Add(p);
                        p = new Tuple<double, double>(data.Key, 180 * item.Value.Phase / Math.PI);
                        PhaseData.Collection.Add(p);
                        break;
                    }
                }
            }
            RaisePropertyChanged("ModuleData");
            RaisePropertyChanged("PhaseData");
        }

        private void AddCurrent(ACSweepSolver sol5, string CurrentName)
        {
            foreach (var data in sol5.Currents)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == CurrentName)
                    {
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value.Magnitude);
                        source1.Collection.Add(p);
                        p = new Tuple<double, double>(data.Key, 90 * item.Value.Phase / Math.PI);
                        source2.Collection.Add(p);
                        break;
                    }
                }
            }
        }


    }
}
