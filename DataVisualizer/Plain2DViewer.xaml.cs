using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using Microsoft.Research.DynamicDataDisplay.DataSources.MultiDimensional;
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

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para Plain2DViewer.xaml
    /// </summary>
    public partial class Plain2DViewer : Window
    {
        
        Circuit cir2;
        ComplexPlainSolver sol1;

        public Plain2DViewer()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            LoadField();
        }

        private void LoadField()
        {
            Circuit cir = new Circuit();
            cir.ReadCircuit("Circuits/RCL.net");
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
            cir2.Setup.Add(ac1);
            ACSweepSolver.Optimize(cir2);
            cir2.Solve();
            sol1 = (ComplexPlainSolver)ac1.Solver;

          
            int Rows = ac1.Points;
            int Columns = ac1.Points;
            double[,] data = new double[Rows, Columns];
            Point[,] gridData = new Point[Rows, Columns];
            MathNet.Numerics.Complex32 W;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    W = sol1.WfromIndexes[new Tuple<int, int>(i, j)];
                    foreach (var node in sol1.Voltages[W])
                    {
                        if (node.Key == "out")
                        {
                            data[i, j] = 20 * Math.Log10(node.Value.Magnitude);
                            //data[i, j] = 180 * node.Value.Phase / Math.PI;
                            gridData[i, j] = new Point(W.Real, W.Imaginary);
                        }
                        
                      
                    }
                }
            
            WarpedDataSource2D<double> dataSource = new WarpedDataSource2D<double>(data, gridData);
            isolineGraph.DataSource = dataSource;
            trackingGraph.DataSource = dataSource;

        }

        private void Button_Zoom(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonRefresh(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAbrir_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
