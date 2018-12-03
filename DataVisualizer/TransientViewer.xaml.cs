using ElectricalAnalysis;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

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
        TransientAnalysis ac5;

        public TransientViewer()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        public void Simulate(string circuitname)
        {
            propgrid.SelectedObject = null;
            cir = new Circuit();
            cir.ReadCircuit(circuitname);
            cir = (Circuit)cir.Clone();
            cir.Setup.RemoveAt(0);
            if (ac5 == null || cir.Setup.Count == 0)
            {
                ac5 = new TransientAnalysis();
                ac5.Step = "50n";
                ac5.FinalTime = "50u";
                cir.Setup.Add(ac5);
            }
            TransientSolver sol5 = (TransientSolver)ac5.Solver;
            //sol5.Optimize(cir2);
            Refresh();
            lbComponents.ItemsSource = cir.Components;
            lbNodes.ItemsSource = cir.Nodes.Values;

        }

        private void Refresh()
        {
            //TransientSolver sol5 = (TransientSolver)cir2.Setup[0].Solver;
            if (cir == null)
            {
                return;
            }
            cir.Solve();
            Redraw();
        }

        private void Redraw()
        {
            if (cir == null || propgrid.SelectedObject == null)
                return;

            if (source1 != null)
                source1.Collection.Clear();

            TransientSolver sol5 = (TransientSolver)cir.Setup[0].Solver;
            if (propgrid.SelectedObject is Node)
            {
                //if (cir.Nodes.ContainsKey(txtPlotted.Text))
                //{
                //    DrawVoltage(sol5, txtPlotted.Text);
                //}
                //else
                //{
                DrawVoltage(sol5, ((Node)propgrid.SelectedObject).Name);
                txtPlotted.Text = ((Node)propgrid.SelectedObject).Name;
                //}
            }
            else if (propgrid.SelectedObject is Dipole)
            {
                DrawCurrent(sol5, ((Dipole)propgrid.SelectedObject).Name);
                txtPlotted.Text = ((Dipole)propgrid.SelectedObject).Name;
                //foreach (var item in cir.Components)
                //{
                //    if (item.Name == txtPlotted.Text)
                //    {
                //        DrawCurrent(sol5, item.Name);
                //        txtPlotted.Text = item.Name;
                //    }
                //}
            }
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
                txtCircuitName.Text = dlg.FileName;
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


        private void Button_AnalysisSetup(object sender, RoutedEventArgs e)
        {
            if (cir == null)
            {
                return;
            }
            propgrid.SelectedObject = cir.Setup[0];
        }


        private void BtnExport(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Net List files (*.csv)|*.csv|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                TransientSolver sol5 = (TransientSolver)cir.Setup[0].Solver;
                sol5.Export(save.FileName);
            }
        }
    }
}
