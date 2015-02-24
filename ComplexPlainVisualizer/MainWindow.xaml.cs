using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
//using MathNet.Numerics;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComplexPlainVisualizer
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ComplexPlainViewModel model;
        Circuit cir2;
        ComplexPlainSolver sol1;

        public MainWindow()
        {
            InitializeComponent();
            model = new ComplexPlainViewModel();
            model.ColorCoding = ColorCoding.ByLights;

            Circuit cir = new Circuit();
            cir.ReadCircuit("Circuits/RCcharge.net");
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
            cir2.Setup.Add(ac1);
            ACSweepSolver.Optimize(cir2);
            cir2.Solve();
            sol1 = (ComplexPlainSolver)ac1.Solver;

            int scalefactor = 2000;
            model.MinX = ac1.SigmaMin / scalefactor;
            model.MaxX = ac1.SigmaMax / scalefactor;
            model.MaxY = ac1.WMax / scalefactor;
            model.MinY = ac1.WMin / scalefactor;
            model.Columns = ac1.Points;
            model.Rows = ac1.Points;
            var data = new Point3D[model.Rows, model.Columns];
            //public Tuple<Complex32, Complex32>[,] Results { get; set; }
            MathNet.Numerics.Complex32 W;
            for (int i = 0; i < model.Rows; i++)
                for (int j = 0; j < model.Columns; j++)
                {
                    W = sol1.WfromIndexes[new Tuple<int, int>(i, j)];
                    foreach (var node in sol1.Results[W])
                    {
                        if (node.Key == "$N_0001")
                            data[i, j] = new Point3D(W.Real /scalefactor, 
                                                    W.Imaginary /scalefactor,
                                                    //node.Value.Magnitude);
                                                    2 * Math.Log10(node.Value.Magnitude));
                    }
                }

            model.Data = data;
            model.UpdateModel(false);

            DataContext = model;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
            //model.CreateDataArray(ModuleInDB);
            
          
        }

    }
}
