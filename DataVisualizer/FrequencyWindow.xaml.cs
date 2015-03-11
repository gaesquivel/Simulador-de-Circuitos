using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MathNet.Numerics;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using ElectricalAnalysis;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using Microsoft.Win32;

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para FrequencyWindow.xaml
    /// </summary>
    public partial class FrequencyWindow : System.Windows.Window
    {
        public static DependencyProperty dp = DependencyProperty.Register("SelectedObject", typeof(object),
            typeof(FrequencyWindow));//, new FrameworkPropertyMetadata(selectedobjectcallback));

        //private static void selectedobjectcallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
            
        //}

        ObservableDataSource<Tuple<double, double>> source1 = null;
        ObservableDataSource<Tuple<double, double>> source2 = null;
        Circuit cir;
        Circuit cir2;

        public object SelectedObject { 
            get{
                return GetValue(dp);
            }
            set {
                SetValue(dp, value);
            }
        }


        public FrequencyWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        public void Simulate(string circuitname)
        {
            TxtStatus.Text = circuitname;
            cir = new Circuit();

            //cir.ReadCircuit("Circuits/RCL.net");
            cir.ReadCircuit(circuitname);
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            ACAnalysis ac = new ACAnalysis();
            cir2.Setup.Add(ac);
            ACSweepSolver.Optimize(cir2);
           
            Refresh();
        }

        private void Refresh()
        {
            ACSweepSolver sol5 = (ACSweepSolver)cir2.Setup[0].Solver;
            cir2.Solve();
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
                        source1.Collection.Add(p);
                        p = new Tuple<double, double>(data.Key, 180 * item.Value.Phase / Math.PI);
                        source2.Collection.Add(p);
                        break;
                    }
                }
            }
        }

        private void AddCurren(ACSweepSolver sol5, string CurrentName)
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


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create first source
            plotter.DataTransform = new Log10Transform();

            double[] xArray = new double[] { 15, 14, 16, 48, 50, 51 };
            double[] yArray = new double[] { 60, 63, 64, 124, 131, 144 };

            var xds = xArray.AsXDataSource();
            var yds = yArray.AsYDataSource();
            //var ds = new CompositeDataSource(xds, yds);
            
            //LineGraph hola = new LineGraph(ds);
            //plotter.Children.Add(hola);
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

            for (int i=0; i<xArray.Length; i++ )
            {
                source1.Collection.Add(new Tuple<double, double>(xArray[i], yArray[i]));
                source2.Collection.Add(new Tuple<double, double>(xArray[i], yArray[i]));
            }

            plotter.MainHorizontalAxis = new HorizontalAxis
            {
                TicksProvider = new LogarithmNumericTicksProvider(10),
                LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => StringUtils.CodeString(info.Tick) }
            };

            plotter.MainVerticalAxis = new VerticalAxis
            {
                TicksProvider = new LogarithmNumericTicksProvider(10),
                LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => StringUtils.CodeString(info.Tick) }
            };
            //axis.LabelProvider.SetCustomFormatter(info => info.Tick.ToString("#.######E+0"));
            //axis.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            //VerticalAxis axis2 = (VerticalAxis)plotter.MainVerticalAxis;
            ////axis.LabelProvider.SetCustomFormatter(info => info.Tick.ToString("#.######E+0"));
            //axis2.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            //HorizontalAxis xAxis = new HorizontalAxis
            //{
            //    TicksProvider = new LogarithmNumericTicksProvider(10),
            //    LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => StringUtils.CodeString(info.Tick) }
            //};
            //plotter.MainHorizontalAxis = xAxis;

            //VerticalAxis yAxis = new VerticalAxis
            //{
            //    TicksProvider = new LogarithmNumericTicksProvider(10),
            //    LabelProvider = new UnroundingLabelProvider()
            //};
            //plotter.MainVerticalAxis = yAxis;



            otherPlotter.DataTransform = new Log10XTransform();
            otherPlotter.MainHorizontalAxis = new HorizontalAxis
            {
                TicksProvider = new LogarithmNumericTicksProvider(10),
                LabelProvider = new UnroundingLabelProvider() { CustomFormatter = info => StringUtils.CodeString(info.Tick) }
            };
            //otherPlotter.MainHorizontalAxis = xAxis2;
            //otherPlotter.MainVerticalAxis = xAxis;

            //yAxis = new VerticalAxis
            //{
            //    TicksProvider = new LogarithmNumericTicksProvider(10),
            //    LabelProvider = new UnroundingLabelProvider()
            //};
            // otherPlotter.MainVerticalAxis = yAxis;



            linegraph.DataSource = source1;
            phasegraph.DataSource = source2;

        }

        private void ButtonAbrir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.InitialDirectory = ".";
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            dlg.Filter = "Circuit Net List files (*.net)|*.net|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                Simulate(dlg.FileName);
            }
        }

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {
            if (source1 != null)
                source1.Collection.Clear();
            if (source2 != null)
                source2.Collection.Clear();
            // Start computation process in second thread
            //Thread simThread = new Thread(new ThreadStart(Simulate));
            //simThread.IsBackground = true;
            //simThread.Start();

            Simulate(TxtStatus.Text);
            lbComponents.ItemsSource = cir.Components;
            lbNodes.ItemsSource = cir.Nodes.Values;

        }

        private void ButtonRefresh(object sender, RoutedEventArgs e)
        {
            source1.Collection.Clear();
            source2.Collection.Clear();
            // Start computation process in second thread
            //Thread simThread = new Thread(new ThreadStart(Refresh));
            //simThread.IsBackground = true;
            //simThread.Start();

            Refresh();
        }


        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == lbComponents)
                propgrid.SelectedObject = lbComponents.SelectedItem;
            else if (sender == lbNodes)
                propgrid.SelectedObject = lbNodes.SelectedItem;
        }

        private void ButtonAddLine(object sender, RoutedEventArgs e)
        {
            if (propgrid.SelectedObject  == null)
                return;
           
            //plotterphase.MainHorizontalAxis = xAxis2;

            if (source1 != null)
                source1.Collection.Clear();  
            if (source2 != null)
                source2.Collection.Clear();  

            ACSweepSolver sol5 = (ACSweepSolver)cir2.Setup[0].Solver;
            if (propgrid.SelectedObject is Node)
            {
                AddVoltage(sol5, ((Node)propgrid.SelectedObject).Name);
            }
            else if (propgrid.SelectedObject is Dipole)
            {
                AddCurren(sol5, ((Dipole)propgrid.SelectedObject).Name);
            }
            
        }



        private void Button_AnalysisSetup(object sender, RoutedEventArgs e)
        {
            if (cir2 == null)
            {
                return;
            }
            propgrid.SelectedObject = cir2.Setup[0];
        }


        private void BtnExport(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Net List files (*.csv)|*.csv|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                //CsvFileWriter writer = new CsvFileWriter(save.FileName);
                //writer.Quote = ';';
                ACSweepSolver sol5 = (ACSweepSolver)cir2.Setup[0].Solver;
                sol5.ExportToCSV(save.FileName);
            }
        }
    }
}
