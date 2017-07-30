using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis.Data;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DataVisualizer.MVVM.ViewModel
{
    public class BodeViewModel: CircuitSimulationViewModel
    {

        ObservableDataSource<Tuple<double, double>> source1 = null;
        ObservableDataSource<Tuple<double, double>> source2 = null;

        [Browsable(false)]
        public ObservableDataSource<Tuple<double, double>> ModuleDataSource
        {
            get { return source1; }
            protected set { RaisePropertyChanged(value, ref source1); }
        }

        [Browsable(false)]
        public ObservableDataSource<Tuple<double, double>> PhaseDataSource
        {
            get { return source2; }
            protected set { RaisePropertyChanged(value, ref source2); }
        }

        [Browsable(false)]
        public ChartPlotter ModulePlotter { get;  set; }
        [Browsable(false)]
        public ChartPlotter PhasePlotter { get; set; }
        [Browsable(false)]
        public LineGraph linegraph { get;  set; }
        [Browsable(false)]
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
            InitializePlotters();
        }

        private void InitializePlotters()
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

        }

        public override void StoragePlot(object obj)
        {
            LineGraph line = obj as LineGraph;

            if (line == null)
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("A Linegraph was expected!", Notification.ErrorType.error));
                return;
            }

            {
                ModuleDataSource.SetXYMapping(z =>
                {
                    Point p = new Point(z.Item1, z.Item2);
                    return p;
                });

                line.DataSource = ModuleDataSource;
                PlottedItems.Add(new Tuple<Plotter2D, LineGraph>(ModulePlotter, line));
            }
        }

        public override void ClearPlots(object obj)
        {
            foreach (var item in PlottedItems)
            {
                LineGraph line = item.Item2;
                if (ModulePlotter.Children.Contains(line))
                    ModulePlotter.Children.Remove(line);
            }
            PhasePlotter.Children.Clear();
            PlottedItems.Clear();
            SelectedPlot = null;
        }

        public override void ShowPlot(object obj)
        {
            if (PlottedItems.Count > 0 &&
                SelectedPlot != null &&
                !ModulePlotter.Children.Contains(SelectedPlot))
                    ModulePlotter.Children.Add(SelectedPlot);
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
                //PlottedItems.Remove(SelectedPlot);
                ModulePlotter.Children.Remove(SelectedPlot);
                SelectedPlot = null;
            }
        }

        protected override bool IsAnalisysType(BasicAnalysis analis)
        {
            return analis is ACAnalysis;
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
                var voltages = ((Tuple<BasicAnalysis, Complex, DataBase>)obj).Item3 as FrequencyData;
                var param = ((Tuple<BasicAnalysis, Complex, DataBase>)obj).Item2;
                ShowVoltage(voltages, name, true);
                Legend.SetDescription(linegraph, name + " " + param.Real.ToString());
                linegraph.ToolTip = name + " - " + param.Real.ToString();
                Legend.SetDescription(phasegraph, name + " " + param.Real.ToString());
                phasegraph.ToolTip = name + " - " + param.Real.ToString();
                return;
            }

            //analisis convencional
            ACAnalysis analis = CurrentAnalisys() as ACAnalysis;
            ACSweepSolver sol5 = analis.Solver as ACSweepSolver;
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
                InitializePlotters();
            }
        }


        //int alfa = 255;
        byte blue = 0;
        //public static double DecrementAlfa(double Original)
        //{

        //}
        int run = 0;
        protected bool ShowVoltage(FrequencyData voltages, string name, bool add = false)
        {
            bool wasfinded = false;
            if (add)
            {
                #region data creation 
                ModuleDataSource = new ObservableDataSource<Tuple<double, double>>();
                PhaseDataSource = new ObservableDataSource<Tuple<double, double>>();
                foreach (var data in voltages)
                {
                    foreach (var item in data.Value)
                    {
                        if (item.Key == name)
                        {
                            wasfinded = true;
                            Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value.Magnitude);
                            ModuleDataSource.Collection.Add(p);
                            p = new Tuple<double, double>(data.Key, 180 * item.Value.Phase / Math.PI);
                            PhaseDataSource.Collection.Add(p);
                        }
                    }
                }
                #endregion
                linegraph = new LineGraph(ModuleDataSource);
                linegraph.Name = "Module_" + name + "_" + (run++).ToString();
                linegraph.Tag = linegraph.Name;
                linegraph.Stroke = new SolidColorBrush(Color.FromRgb(0, blue += 50, 255)); //BrushHelper.MakeTransparent(Brushes.Blue, alfa-=30);
                linegraph.StrokeThickness = 3;

                phasegraph = new LineGraph(PhaseDataSource);
                phasegraph.Name = "Phase_" + name + "_" + run.ToString();
                phasegraph.Tag = phasegraph.Name;
                phasegraph.Stroke = new SolidColorBrush(Color.FromRgb(0, blue, 255)); //BrushHelper.MakeTransparent(Brushes.Blue, alfa-=30);
                phasegraph.StrokeThickness = 3;

                InitializePlotters();
                StoragePlot(linegraph);

                ModulePlotter.Children.Add(linegraph);
                PhasePlotter.Children.Add(phasegraph);
            }
           

            return wasfinded;
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
                        ModuleDataSource.Collection.Add(p);
                        p = new Tuple<double, double>(data.Key, 180 * item.Value.Phase / Math.PI);
                        PhaseDataSource.Collection.Add(p);
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
