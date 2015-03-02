using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using MathNet.Numerics;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Microsoft.Research.DynamicDataDisplay.DataSources;
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
    public partial class MainWindow :  System.Windows.Window
    {
        ObservableDataSource<Tuple<double, double>> source1 = null;
        //ObservableDataSource<Tuple<double, double>> sourcevoltage = null;
        Circuit cir;
        Circuit cir2;
        //VerticalAxisTitle axis;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
           
        }

        public void Simulate()
        {
            cir = new Circuit();
//            cir.ReadCircuit("Circuits/RCL.net");
            cir.ReadCircuit("Circuits/vsingain.net");
            //cir.ReadCircuit("Circuits/RCcharge.net");
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            TransientAnalysis ac5 = new TransientAnalysis();
            ac5.Step = "20u";
            ac5.FinalTime = "3m";
            cir2.Setup.Add(ac5);
            TransientSolver sol5 = (TransientSolver)ac5.Solver;
            TransientSolver.Optimize(cir2);
            Refresh();
           // Redraw();

        }

        private void Refresh()
        {
            TransientSolver sol5 = (TransientSolver)cir2.Setup[0].Solver;
            cir2.Solve();
            Redraw();

            //DrawCurrent(sol5, "C1");
           // DrawVoltage(sol5, "out");

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
            source1.SetXYMapping(z => 
                {
                    Point p = new Point(z.Item1, z.Item2);
                    return p;
                });
            //sourcevoltage = new ObservableDataSource<Tuple<double, double>>();
            //sourcevoltage.SetXYMapping(z =>
            //{
            //    Point p = new Point(z.Item1, z.Item2);
            //    return p;
            //});

            Random rnd = new Random();
            for (int i = 0; i < 50; i++)
            {
                source1.Collection.Add(new Tuple<double, double>(i, rnd.Next(100)));
                //sourcevoltage.Collection.Add(new Tuple<double, double>(i, 10 + rnd.Next(20)));
            }
            //LineGraph line = new LineGraph(source1);
            //line.AddToPlotter(plotter);
           // plotter.Children.Add(line);

            linegraph.DataSource = source1;
          //  linephase.DataSource = sourcevoltage;
            //axis = new VerticalAxisTitle();
            //axis.da
            //plotter.Children.Add(axis);
            //linegraph.AddToPlotter(plotter);
            //RightVerticalAxis.
        }

        private void ButtonAbrir_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {
            source1.Collection.Clear();
            //source1.Collection.Clear();
            // Start computation process in second thread
            //Thread simThread = new Thread(new ThreadStart(Simulate));
            //simThread.IsBackground = true;
            //simThread.Start();
            
            Simulate();
            lbComponents.ItemsSource = cir.Components;
            lbNodes.ItemsSource = cir.Nodes.Values;

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

        private void Button_Zoom(object sender, RoutedEventArgs e)
        {
           
           // propgrid.SelectedObject = plotter.AxisGrid;
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

        private void ButtonAddLine(object sender, RoutedEventArgs e)
        {
            Redraw();

        }

        private void Redraw()
        {
            if (propgrid.SelectedObject == null)
                return;

            if (source1 != null)
                source1.Collection.Clear();


            TransientSolver sol5 = (TransientSolver)cir2.Setup[0].Solver;
            if (propgrid.SelectedObject is Node)
            {
                //if (sourcevoltage != null)
                //    sourcevoltage.Collection.Clear();
                DrawVoltage(sol5, ((Node)propgrid.SelectedObject).Name);
            }
            else if (propgrid.SelectedObject is Dipole)
            {
                DrawCurrent(sol5, ((Dipole)propgrid.SelectedObject).Name);
            }
        }

        private void Button_AnalysisSetup(object sender, RoutedEventArgs e)
        {
            propgrid.SelectedObject = cir2.Setup[0];
        }

    }
}
