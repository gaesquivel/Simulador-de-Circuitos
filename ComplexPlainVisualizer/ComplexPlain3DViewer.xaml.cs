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
using Microsoft.Win32;

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

            model.ColorCoding = ColorCoding.ByLights;
            Button_Click(this, null);
            Simulate("Circuits/RCL.net");
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {


            //propgrid.SelectedObject = cir2;
        }

        private void Update(ComplexPlainAnalysis ac1)
        {
            cir2.Solve();
            sol1 = (ComplexPlainSolver)ac1.Solver;

            model.MinX = ac1.SigmaMin;
            model.MaxX = ac1.SigmaMax;
            model.MaxY = ac1.WMax;
            model.MinY = ac1.WMin ;
            model.Columns = ac1.Points;
            model.Rows = ac1.Points;
           

            var data = new Point3D[model.Rows + 1, model.Columns +1];
            MathNet.Numerics.Complex32 W;
            for (int i = 0; i <= model.Rows; i++)
                for (int j = 0; j <= model.Columns; j++)
                {
                    W = sol1.WfromIndexes[new Tuple<int, int>(i, j)];
                    foreach (var node in sol1.Voltages[W])
                    {
                        if (node.Key == "out")
                            data[i, j] = new Point3D(W.Real,
                                                    W.Imaginary,
                               // node.Value.Magnitude);
                                                    20 * Math.Log10(node.Value.Magnitude));
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
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);;
            dlg.Filter = "Circuit Net List files (*.net)|*.net|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                Simulate(dlg.FileName);
            }
        }

        private void Simulate(string FileName)
        {
            cir = new Circuit();
            cir.ReadCircuit(FileName);
            cir2 = (Circuit)cir.Clone();
            cir2.Setup.RemoveAt(0);
            ComplexPlainAnalysis ac1 = new ComplexPlainAnalysis();
            cir2.Setup.Add(ac1);
            ACSweepSolver.Optimize(cir2);

            Update(ac1);

            lbComponents.ItemsSource = cir.Components;
            lbNodes.ItemsSource = cir.Nodes;
            //lbComponents.SelectedItem
            DataContext = model;
        }

        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == lbComponents)
                propgrid.SelectedObject = lbComponents.SelectedItem;
            else if (sender == lbNodes)
                propgrid.SelectedObject = lbNodes.SelectedItem;
        }

    }
}
