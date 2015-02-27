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
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using System.Windows.Media.Media3D;

namespace ComplexPlainVisualizer
{
    /// <summary>
    /// Lógica de interacción para ComplexPlain3DViewer.xaml
    /// </summary>
    public partial class ComplexPlain3DViewer : Window
    {
        public ComplexPlainViewModel model { get; set; }
        Circuit cir2, cir;
        ComplexPlainSolver sol1;



        public ComplexPlain3DViewer()
        {
            InitializeComponent();
            model = new ComplexPlainViewModel();
            cir = new Circuit();

            Button_Click(this, null);

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            model.ColorCoding = ColorCoding.ByLights;


            cir.ReadCircuit("Circuits/RCL.net");
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
            cir2.Setup.Add(ac1);
            ACSweepSolver.Optimize(cir2);

            Update(ac1);

            lbComponents.ItemsSource = cir.Components;
            //lbComponents.SelectedItem
            DataContext = model;
            //propgrid.SelectedObject = cir2;
        }

        private void Update(ComplexPlainAnalysis ac1)
        {
            cir2.Solve();
            sol1 = (ComplexPlainSolver)ac1.Solver;

            int scalefactor = 5000;
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
                    foreach (var node in sol1.Voltages[W])
                    {
                        if (node.Key == "out")
                            data[i, j] = new Point3D(W.Real / scalefactor,
                                                    W.Imaginary / scalefactor,
                                //node.Value.Magnitude);
                                                    2 * Math.Log10(node.Value.Magnitude));
                    }
                }

            model.Data = data;
            model.UpdateModel(false);
        }

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {
            Update((ComplexPlainAnalysis)cir2.Setup[0]);
        }

        private void ButtonAbrir_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
