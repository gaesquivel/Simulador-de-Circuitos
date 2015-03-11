using ElectricalAnalysis;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using MathNet.Numerics;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class TransientViewer :  System.Windows.Window
    {
        ObservableDataSource<Tuple<double, double>> source1 = null;
        //ObservableDataSource<Tuple<double, double>> sourcevoltage = null;
        Circuit cir;
        Circuit cir2;
        TransientAnalysis ac5;

        public TransientViewer()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        public void Simulate(string circuitname)
        {
            cir = new Circuit();
            cir.ReadCircuit(circuitname);
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            if (ac5 == null || cir2.Setup.Count == 0)
            {
                ac5 = new TransientAnalysis();
                ac5.Step = "200n";
                ac5.FinalTime = "50u";
                cir2.Setup.Add(ac5);
            }
            TransientSolver sol5 = (TransientSolver)ac5.Solver;
            TransientSolver.Optimize(cir2);
            Refresh();
        }

        private void Refresh()
        {
            //TransientSolver sol5 = (TransientSolver)cir2.Setup[0].Solver;
            cir2.Solve();
            Redraw();
        }

        private void DrawVoltage(TransientSolver sol5, string name)
        {
            foreach (var data in sol5.Voltages)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == name)
                    {
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value);
                        source1.Collection.Add(p);
                    }
                }
            }
        }

        private void DrawCurrent(TransientSolver sol5, string name)
        {
            foreach (var data in sol5.Currents)
            {
                foreach (var item in data.Value)
                {
                    if (item.Key == name)
                    {
                        Tuple<double, double> p = new Tuple<double, double>(data.Key, item.Value);
                        source1.Collection.Add(p);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create first source
            source1 = new ObservableDataSource<Tuple<double, double>>();
            source1.SetXYMapping(z =>   {
                                            Point p = new Point(z.Item1, z.Item2);
                                            return p;
                                        });
            HorizontalAxis axis = (HorizontalAxis)plotter.MainHorizontalAxis;
            //axis.LabelProvider.SetCustomFormatter(info => info.Tick.ToString("#.######E+0"));
            axis.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            VerticalAxis axis2 = (VerticalAxis)plotter.MainVerticalAxis;
            //axis.LabelProvider.SetCustomFormatter(info => info.Tick.ToString("#.######E+0"));
            axis2.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            Random rnd = new Random();
            for (int i = 0; i < 50; i++)
            {
                source1.Collection.Add(new Tuple<double, double>(i, rnd.Next(100)));
                //sourcevoltage.Collection.Add(new Tuple<double, double>(i, 10 + rnd.Next(20)));
            }

            linegraph.DataSource = source1;
        }

        private void ButtonAbrir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.InitialDirectory = ".";
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            dlg.Filter = "Circuit Net List files (*.net)|*.net|All files (*.*)|*.*";
            if (dlg.ShowDialog(this).Value == true)
            {
                Simulate(dlg.FileName);
            }
        }

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {
            source1.Collection.Clear();
            //source1.Collection.Clear();
            // Start computation process in second thread
            //Thread simThread = new Thread(new ThreadStart(Simulate));
            //simThread.IsBackground = true;
            //simThread.Start();
            
            Simulate(txtCircuitName.Text);
            lbComponents.ItemsSource = cir.Components;
            lbNodes.ItemsSource = cir.Nodes.Values;
            DataContext = ((TransientSolver)ac5.Solver).Voltages;
        }

        private void ButtonRefresh(object sender, RoutedEventArgs e)
        {
            source1.Collection.Clear();
            // Start computation process in second thread
            //Thread simThread = new Thread(new ThreadStart(Refresh));
            //simThread.IsBackground = true;
            //simThread.Start();
 
            Refresh();
        }

        

        private void Button_Bode(object sender, RoutedEventArgs e)
        {
            FrequencyWindow win = new FrequencyWindow();
            win.Show();
        }

        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == lbComponents)
                propgrid.SelectedObject = lbComponents.SelectedItem;
            else if (sender == lbNodes)
                propgrid.SelectedObject = lbNodes.SelectedItem;
        }

        private void ButtonRedraw(object sender, RoutedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {

            if (cir2 == null)
                return;

            if (source1 != null)
                source1.Collection.Clear();

            TransientSolver sol5 = (TransientSolver)cir2.Setup[0].Solver;
            if (propgrid.SelectedObject is Node)
            {
                //if (sourcevoltage != null)
                //    sourcevoltage.Collection.Clear();
                if (cir.Nodes.ContainsKey(txtPlotted.Text))
                    DrawVoltage(sol5, txtPlotted.Text);

//                DrawVoltage(sol5, ((Node)propgrid.SelectedObject).Name);
            }
            else if (propgrid.SelectedObject is Dipole)
            {
                foreach (var item in cir.Components)
	            {
                    if (item.Name == txtPlotted.Text)
                        DrawCurrent(sol5, item.Name);
                //DrawCurrent(sol5, ((Dipole)propgrid.SelectedObject).Name);
		 
	            }
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
                ACSweepSolver sol5 = (ACSweepSolver)cir2.Setup[0].Solver;
                sol5.ExportToCSV(save.FileName);
            }
        }
    }
}
