using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using DataVisualizer;
using System.Numerics;
using DataVisualizer.MVVM.ViewModel;

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para ComplexPlain3DViewer.xaml
    /// </summary>
    public partial class ComplexPlain3DViewer : System.Windows.Window
    {
        public ComplexPlainViewModel1 model { get; set; }
        Circuit cir;
        ComplexPlainSolver sol1;
        ComplexPlainAnalysis ac1;
        //System.Drawing.Bitmap CurrentBitmap;


        public ComplexPlain3DViewer()
        {
            InitializeComponent();
            model = new ComplexPlainViewModel1();

            model.ColorCoding = ColorCoding.ByGradientY;
            model.SurfaceBitmap = Properties.Resources.planocomplejo; ;
           


            Simulate(TxtCircuitFile.Text);
        }
        
        private void Update()
        {
            cir.Solve();
            Redraw(txtPlotted.Text);
        }

        private Point3D[,] Redraw(string nodename)
        {
            var data = new Point3D[model.Rows + 1, model.Columns + 1];
            var dataOriginal = new Point4D[model.Rows + 1, model.Columns + 1];
            Complex W;
            for (int i = 0; i <= model.Rows; i++)
                for (int j = 0; j <= model.Columns; j++)
                {
                    W = sol1.WfromIndexes[new Tuple<int, int>(i, j)];
                    foreach (var node in sol1.Voltages[W])
                    {
                        if (node.Key == nodename)
                        {
                            data[i, j] = new Point3D(W.Real,
                                                    W.Imaginary,
                                //node.Value.Magnitude);
                                                    Math.Log10(node.Value.Magnitude));
                            dataOriginal[i, j] = new Point4D(W.Real, W.Imaginary,
                                                            node.Value.Magnitude, node.Value.Phase);

                        }
                    }
                }
            surface.OriginalData = dataOriginal;
            model.Data = data;
            model.UpdateModel(false);

            return data;
        }

        private void Simulate(string FileName)
        {
            cir = new Circuit();
            cir.ReadCircuit(FileName);
            cir.Setup.RemoveAt(0);
            ac1 = new ComplexPlainAnalysis();
            cir.Setup.Add(ac1);
            sol1 = (ComplexPlainSolver)ac1.Solver;
            model.MinX = ac1.SigmaMin;
            model.MaxX = ac1.SigmaMax;
            model.MaxY = ac1.WMax;
            model.MinY = ac1.WMin;

            model.Columns = ac1.Points;
            model.Rows = ac1.Points;

            Update();

            lbComponents.ItemsSource = cir.Components;
            lbNodes.ItemsSource = cir.Nodes.Values;
            //lbComponents.SelectedItem
            DataContext = model;
            TxtEditor.Text = cir.ToString();
        }

        #region buttons

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {
            Update();
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

        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == lbComponents)
                propgrid.SelectedObject = lbComponents.SelectedItem;
            else if (sender == lbNodes)
                propgrid.SelectedObject = lbNodes.SelectedItem;
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
                ComplexPlainSolver sol5 = (ComplexPlainSolver)cir.Setup[0].Solver;
                int index = 1;
                //if (lbNodes.SelectedItem != null)
                //    sol5.SelectedNode = (Node)lbNodes.SelectedItem;

                index = lbNodes.SelectedIndex;
                sol5.Export(save.FileName);
            }
        }

        private void ButtonRedraw(object sender, RoutedEventArgs e)
        {
            if (lbNodes.SelectedItem == null)
            {
                return;
            }
            txtPlotted.Text = ((Node)lbNodes.SelectedItem).Name;
            Redraw(txtPlotted.Text);
        }



        private void ButtonExportPhase(object sender, RoutedEventArgs e)
        {
            if (lbNodes.SelectedItem == null)
                return;

            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Net List files (*.bmp)|*.bmp|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                //System.Drawing.Bitmap bmp = CreateBitmap();
                //bmp.Save(save.FileName);
            }
        }

        //private System.Drawing.Bitmap CreateBitmap()
        //{
        //    System.Drawing.Bitmap bmp = sol1.TakeSnapShot((Node)lbNodes.SelectedItem); //FileUtils.DrawImage(func, ac1.Points, ac1.Points);
        //    image.Source = ImageUtils.Bitmap2BitmapSource(bmp);
        //    return bmp;
        //}

        private void ButtonColorize(object sender, RoutedEventArgs e)
        {
            if (lbNodes.SelectedItem == null)
                return;
            surface.ColorCoding = model.ColorCoding = ColorCoding.Custom;
            model.UpdateModel(false);
        }

        private void PlainDoubleClick(object sender, MouseButtonEventArgs e)
        {
            propgrid.SelectedObject = model;
        }

        private void ButtonShowBode(object sender, RoutedEventArgs e)
        {
            FrequencyWindow win = new FrequencyWindow();
            win.Show();
        }

        private void ButtonShowTransient(object sender, RoutedEventArgs e)
        {
            TransientViewer win = new TransientViewer();
            win.Show();
        }

        #endregion


        Complex func(int x, int y)
        {
            Complex W = sol1.WfromIndexes[new Tuple<int, int>(x, y)];
            foreach (var node in sol1.Voltages[W])
            {
                if (node.Key == txtPlotted.Text)
                {
                    return node.Value;
                }
            }
            return Complex.Zero;
        }

    }
}
