using CircuitMVVMBase.Commands;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using HelixToolkit.Wpf;
using Microsoft.Research.DynamicDataDisplay.DataSources.MultiDimensional;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace DataVisualizer.MVVM.ViewModel
{
    public class ComplexPlainViewModel : CircuitSimulationViewModel
    {
        public enum ModuleScale
        { deciBell, normal, log10 }

        private ModuleScale moduleunit;
        [Browsable(false)]
        public SurfacePlotVisual3D surface { get; set; }

        //[Browsable(false)]
        public Range<double> XRange { get; set; }
        //[Browsable(false)]
        public Range<double> YRange { get; set; }

        [Browsable(false)]
        public int Rows { get; set; }
        [Browsable(false)]
        public int Columns { get; set; }


        List<System.Drawing.Bitmap> bitmaps;
        [Browsable(false)]
        public System.Drawing.Bitmap SurfaceBitmap { get; set; }

        BitmapSource _FaseBitmap;
        public BitmapSource FaseBitmap {
            get { return _FaseBitmap; }
            set { RaisePropertyChanged(value, ref _FaseBitmap); }
        }

        WarpedDataSource2D<double> isolinesDataSource;
        public WarpedDataSource2D<double> IsoLinesDataSource {
            get { return isolinesDataSource; }
            protected set { RaisePropertyChanged(value, ref isolinesDataSource); }
        }


        public Viewport3D ViewPort { get; set; }

        [Browsable(false)]
        public Func<double, double, double> Function { get; set; }
        [Browsable(false)]
        public Point3D[,] Data { get; set; }
        [Browsable(false)]
        public double[,] ColorValues { get; set; }

        ColorCoding colorcoding;
        public ColorCoding ColorCoding {
            get { return colorcoding; }
            set {
                if (RaisePropertyChanged(value, ref colorcoding))
                {
                    if (surface != null)
                        surface.ColorCoding = colorcoding;
                    switch (value)
                    {
                        case ColorCoding.ModuleBlue:
                            SurfaceBitmap = bitmaps[2];
                            break;
                        case ColorCoding.ModuleFire:
                            SurfaceBitmap = bitmaps[6];
                            break;
                        case ColorCoding.ModuleLevel:
                            SurfaceBitmap = bitmaps[4];
                            break;
                        case ColorCoding.Fase1:
                            SurfaceBitmap = bitmaps[7];
                            break;
                        case ColorCoding.Fase2:
                            SurfaceBitmap = bitmaps[8];
                            break;
                        case ColorCoding.Fase3:
                            SurfaceBitmap = bitmaps[0];
                            break;
                       
                        case ColorCoding.PhaseBlue:
                            SurfaceBitmap = bitmaps[1];
                            break;
                        case ColorCoding.PhaseFire:
                            SurfaceBitmap = bitmaps[5];
                            break;
                        case ColorCoding.Phaselevel:
                            SurfaceBitmap = bitmaps[3];
                            break;
                        default:
                            break;
                    }
                    Redraw(false);
                }
            }
        }

        [Browsable(false)]
        public Model3DGroup Lights
        {
            get
            {
                var group = new Model3DGroup();
                switch (ColorCoding)
                {
                    case ColorCoding.ByGradientY:
                    case ColorCoding.Fase1:
                        group.Children.Add(new AmbientLight(Colors.White));
                        break;
                    case ColorCoding.ByLights:
                        group.Children.Add(new AmbientLight(Colors.Gray));
                        group.Children.Add(new PointLight(Colors.Red, new Point3D(0, -1000, 0)));
                        group.Children.Add(new PointLight(Colors.Blue, new Point3D(0, 0, 1000)));
                        group.Children.Add(new PointLight(Colors.Green, new Point3D(1000, 1000, 0)));
                        break;
                }
                return group;
            }
        }

        [Browsable(false)]
        public Brush SurfaceBrush
        {
            get
            {
                // Brush = BrushHelper.CreateGradientBrush(Colors.White, Colors.Blue);
                //return GradientBrushes.Hue;
                //return new ImageUtils.CreateComplexBrush();
                //return GradientBrushes.BlueWhiteRed;
                // Brush = GradientBrushes.BlueWhiteRed;
                switch (ColorCoding)
                {
                    case ColorCoding.ByGradientY:
                        return GradientBrushes.Rainbow;
                    //return BrushHelper.CreateGradientBrush(Colors.Red, Colors.White, Colors.Blue);
                    case ColorCoding.ByLights:
                        return GradientBrushes.Hue;
                    //case ColorCoding.Fase1:
                    //case ColorCoding.Fase2:
                    //case ColorCoding.Fase3:
                    //case ColorCoding.Phaselevel:
                    //case ColorCoding.PhaseFire:
                    //case ColorCoding.PhaseBlue:
                    default:
                        if (SurfaceBitmap != null)
                            return ImageUtils.CreateComplexBrush(SurfaceBitmap);
                        else
                            return GradientBrushes.Rainbow;

                }
                //return null;
            }
        }

        public ModuleScale ModuleUnit
        {
            get { return moduleunit; }
            set {
                if (RaisePropertyChanged(value, ref moduleunit))
                    Redraw(false);
            }
        }

        RelayCommand cmdexportfase, cmdshowisolines;
        public RelayCommand ExportPhaseCommand
        {
            get
            {
                return cmdexportfase ?? (cmdexportfase = new RelayCommand(ExportPhase, (x) => {
                    return SelectedObject != null && SelectedObject is NodeSingle;
                    }
                    ));
                //if (cmdexportfase == null)
                //    return cmdexportfase = new RelayCommand(ExportPhase, (x) => {
                //        return SelectedObject != null && SelectedObject is NodeSingle;
                //        }
                //    );
                //return cmdexportfase;
            }

        }


        public RelayCommand ShowIsoLinesCommand
        {
            get
            {
                return cmdshowisolines ?? (cmdshowisolines = new RelayCommand(CreateIsoLines, (x) => {
                    return Data != null;
                }
                    ));
            }

        }


        /// <summary>
        /// Create data from complex plain transfer function to draw isolines
        /// </summary>
        /// <param name="obj"></param>
        private void CreateIsoLines(object obj)
        {
            CreatePhaseBitmap();
            int width = surface.OriginalData.GetUpperBound(0) + 1, height = surface.OriginalData.GetUpperBound(1) + 1;

            double[,] data = new double[width, height];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    double d;
                    if (ColorCoding >= ColorCoding.Fase1)
                    {
                        d = surface.OriginalData[row, column].W;
                        d *= 180 / Math.PI;
                    }
                    else
                        d = surface.OriginalData[row, column].Z;
                    data[column, row] = d;
                }
            }

            Point[,] gridData = new Point[width, height];
            for (int row = 0; row < height; row++)
            {
               
                for (int column = 0; column < width; column++)
                {
                    gridData[column, row] = new Point(Data[row,column].X,
                        Data[row, column].Y);
                }
            }

            IsoLinesDataSource = new WarpedDataSource2D<double>(data, gridData);
        }

        public ComplexPlainViewModel() : base()
        {
            Name = "Complex Plain";
            ShortDescription = "";
            ColorCoding = ColorCoding.ByGradientY;
            //dataSource = new WarpedDataSource2D<double>(data, gridData);
            bitmaps = new List<System.Drawing.Bitmap>();
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.planocomplejoold);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.FaseAzul);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.AzulModulo);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.FaseLvl);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.ModuloLvl);

            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.faseFire);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.modulofire);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.fase4);
            bitmaps.Add(ComplexPlainVisualizer.Properties.Resources.fase5);
            SurfaceBitmap = bitmaps[0];
            Function = (x, y) => Math.Sin(x * y) * 0.5;
            XRange = new Range<double>(0, 3);
            YRange = new Range<double>(0, 3);
            Rows = 91;
            Columns = 91;
            MainObjects.Add(this);
            //SurfaceBitmap = Properties.Resources.planocomplejo; ;

            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 10000;
            myDoubleAnimation.Name = "Animn1";
            myDoubleAnimation.To = 100000;
            myDoubleAnimation.By = 10000;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
            myDoubleAnimation.AutoReverse = true;
            myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            animation = myDoubleAnimation;
            MainObjects.Add(myDoubleAnimation);

            CopyToCommand.CanExecuteTarget = (x) => {
                return ViewPort != null;
            };
            SaveBitmapCommand.CanExecuteTarget = (x) => {
                return ViewPort != null;
            };
            // MainObjects.Add(surface);

            Redraw(true);
        }

        protected override bool IsAnalisysType(BasicAnalysis analis)
        {
            return analis is ComplexPlainAnalysis;
        }


        protected override void Redraw(object obj)
        {
            IsBusy = true;
            if (CurrentCircuit != null)
            {
                if (CurrentCircuit.HasErrors)
                    return;
                if (SelectedObject is NodeSingle || SelectedObject is Dipole)
                    Draw(((Item)SelectedObject).Name);
                else if (CurrentCircuit.Nodes.ContainsKey("out"))
                {
                    Draw("out");
                    PlottedItem = CurrentCircuit.Nodes["out"];
                }
            }

            bool CreateData = false;
            if (obj is bool)
                CreateData = (bool)obj;
            if (CreateData || Data == null)
                Data = CreateDataArray(Function);


            switch (ColorCoding)
            {
                case ColorCoding.ByGradientY:
                    ColorValues = FindGradientZ(Data);
                    break;
                case ColorCoding.ByLights:
                    ColorValues = FindGradientY(Data);
                    break;
                //    ColorValues = null;//CreatePatternGradient(OriginalData);
                default:
                    ColorValues = null;
                    break;
            }

            RaisePropertyChanged("Data");
            RaisePropertyChanged("ColorValues");
            RaisePropertyChanged("SurfaceBrush");
            IsBusy = false;
        }

        public override void Simulate(object obj)
        {
            IsBusy = true;
            string file = "";
            ComplexPlainAnalysis ac1 = null;
            if (CurrentCircuit != null)
                ac1 = CurrentAnalisys() as ComplexPlainAnalysis;
                //ac1 = (from sol in CurrentCircuit.Setup
                //        where sol is ComplexPlainAnalysis
                //        select sol ) as ComplexPlainAnalysis;
            if (ac1 == null)
                ac1 = new ComplexPlainAnalysis();

            if (CurrentCircuit == null || obj is string)
            {
                CurrentCircuit = new Circuit();
                if (CurrentCircuit.Setup[0] is DCAnalysis)
                    CurrentCircuit.Setup.RemoveAt(0);
                CurrentCircuit.Setup.Add(ac1);
                if (!MainObjects.Contains(ac1))
                    MainObjects.Add(ac1);
            }

            if (obj is string)
            {
                file = obj as string;
                CurrentCircuit.ReadCircuit(file);
            }
            //if (CurrentCircuit.IsChanged)
            if (!CurrentCircuit.Parse())
            {
                Notifications.Add(new CircuitMVVMBase.Notification("Error in parsing", CircuitMVVMBase.Notification.ErrorType.error));
                return;
            }
            IsBusy = CurrentCircuit.Solve(ac1);
            double valmin = 0, valmax = 0;
            bool haserror = false;
            if (StringUtils.DecodeString(ac1.SigmaMax, out valmax) &&
                StringUtils.DecodeString(ac1.SigmaMin, out valmin))
                XRange = new Range<double>(valmin, valmax);
            else
                haserror = true;
            if (StringUtils.DecodeString(ac1.WMax, out valmax) &&
                StringUtils.DecodeString(ac1.WMin, out valmin))
                YRange = new Range<double>(valmin, valmax);
            else
                haserror &= true;

            Columns = ac1.Points;
            Rows = ac1.Points;

            Redraw(false);
           // IsBusy = false;
        }

        protected override void Animate(object obj)
        {
            {
                if (SelectedObject is ViewModelBase)
                {
                    AnimationTarget = SelectedObject as ViewModelBase;
                    // Create a name scope for the page.
                    //NameScope.SetNameScope(this, new NameScope());
                    //RegisterName(AnimationTarget.Name, AnimationTarget);

                    myStoryboard.Children.Add(animation);
                    Storyboard.SetTarget(animation, AnimationTarget);
                   // Storyboard.SetTargetName(AnimationTarget, ((ElectricComponent)AnimationTarget).Name);
                    //Storyboard.SetTargetProperty(AnimationTarget, new PropertyPath("Value"));
                    Storyboard.SetTargetProperty(AnimationTarget, new PropertyPath(ElectricComponent.valorDP));
                    // animation.CurrentTimeInvalidated += animationchanged;
                }
            }

        }



        private Point3D[,] Draw(string nodename)
        {
            var anal1 = CurrentAnalisys() as ComplexPlainAnalysis;
            //(from sol in CurrentCircuit.Setup 
            //where sol is ComplexPlainAnalysis select sol) as ComplexPlainAnalysis;
            ComplexPlainSolver sol1 = anal1.Solver as ComplexPlainSolver;
            var data = new Point3D[Rows , Columns];
            var dataOriginal = new Point4D[Rows, Columns];
            Complex S;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    S = sol1.WfromIndexes[new Tuple<int, int>(i, j)];
                    foreach (var node in sol1.Voltages[S])
                    {
                        if (node.Key == nodename)
                        {
                            Complex original = node.Value;
                            double valor = node.Value.Magnitude, fase = 0;
                            if (double.IsNaN(valor))
                            {
                                FindLimitValue(sol1, nodename, i, j, Rows, Columns, ref valor, ref fase);
                            }

                            if (ModuleUnit != ModuleScale.normal)
                            {
                                if (valor == 0)
                                {
                                    FindLimitValue(sol1, nodename, i, j, Rows, Columns, ref valor, ref fase);
                                    Notifications.Add(new CircuitMVVMBase.Notification(
                                        "Zero in Log scale at i=" + i.ToString() +
                                        ", j=" + j.ToString()));
                                }
                                valor = Math.Log10(valor);
                            }
                            if (ModuleUnit == ModuleScale.deciBell)
                            {
                                valor = 20 * valor;
                            }

                            data[i, j] = new Point3D(S.Real,
                                                    S.Imaginary,
                                                    valor);
                            dataOriginal[i, j] = new Point4D(S.Real, S.Imaginary,
                                                            valor, original.Phase);
                        }
                    }
                }
            surface.OriginalData = dataOriginal;
            Data = data;
            return data;
        }

        /// <summary>
        /// Find e given module value like average of
        /// rounding values
        /// </summary>
        /// <param name="sol1"></param>
        /// <param name="nodename"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private void FindLimitValue(ComplexPlainSolver sol1, string nodename, 
                                    int row, int column, int maxRows, int maxColumns,
                                    ref double valor, ref double fase)
        {
            double real = 0, imag = 0;
            Complex W;
            int beginx = 0, endx, beginy = 0, endy;
            if (row > 0)
                beginx = row - 1;
            if (row < maxRows - 1)
                endx = row + 2;
            else
                endx = maxRows;
            if (column > 0)
                beginy = column - 1;
            if (column < maxColumns - 1)
                endy = column + 2;
            else
                endy = maxColumns;

            for (int i = beginx; i < endx; i++)
                for (int j = beginy; j < endy; j++)
                {
                    if (i == row && j == column)
                        continue;

                    W = sol1.WfromIndexes[new Tuple<int, int>(i, j)];
                    
                    foreach (var node in sol1.Voltages[W])
                    {
                        if (node.Key == nodename)
                        {
                            real += node.Value.Magnitude;
                            imag += node.Value.Phase;
                        }
                    }
                }
            valor = real / 8;
            fase = imag / 8;
            //return c;
        }

        private Point GetPointFromIndex(int i, int j)
        {
            //double x = MinX + (double)j / (Columns - 1) * (MaxX - MinX);
            //double y = MinY + (double)i / (Rows - 1) * (MaxY - MinY);
            double x = XRange.Minimum + (double)j / (Columns - 1) * (XRange.Maximum - XRange.Minimum);
            double y = YRange.Minimum + (double)i / (Rows - 1) * (YRange.Maximum - YRange.Minimum);

            return new Point(x, y);
        }

        private Point3D[,] CreateDataArray(Func<double, double, double> f)
        {

            var data = new Point3D[Rows, Columns];
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    var pt = GetPointFromIndex(i, j);
                    data[i, j] = new Point3D(pt.X, pt.Y, f(pt.X, pt.Y));
                }
            return data;
        }

        public static double[,] FindGradientZ(Point3D[,] data, int offset = 0)
        {
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            int n = data.GetUpperBound(0) + 1;
            int m = data.GetUpperBound(0) + 1;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    double z = data[i, j].Z;
                    if (double.IsNaN(z))
                        continue;
                    else
                    {
                        maxZ = Math.Max(maxZ, z);
                        minZ = Math.Min(minZ, z);
                    }
                }


            var K = new double[n, m];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    K[i, j] = MathUtil.Scale(minZ, maxZ, data[i, j].Z, n);
                }

            return K;
        }

        // http://en.wikipedia.org/wiki/Numerical_differentiation
        public static double[,] FindGradientY(Point3D[,] data)
        {
            int n = data.GetUpperBound(0) + 1;
            int m = data.GetUpperBound(0) + 1;
            var K = new double[n, m];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    // Finite difference approximation
                    var p10 = data[i + 1 < n ? i + 1 : i, j - 1 > 0 ? j - 1 : j];
                    var p00 = data[i - 1 > 0 ? i - 1 : i, j - 1 > 0 ? j - 1 : j];
                    var p11 = data[i + 1 < n ? i + 1 : i, j + 1 < m ? j + 1 : j];
                    var p01 = data[i - 1 > 0 ? i - 1 : i, j + 1 < m ? j + 1 : j];

                    //double dx = p01.X - p00.X;
                    //double dz = p01.Z - p00.Z;
                    //double Fx = dz / dx;

                    double dy = p10.Y - p00.Y;
                    double dz = p10.Z - p00.Z;

                    K[i, j] = dz / dy;
                }
            return K;
        }

        private void ExportPhase(object obj)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Net List files (*.bmp)|*.bmp|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                System.Drawing.Bitmap bmp = CreatePhaseBitmap();
                bmp.Save(save.FileName);
            }
        }

        private System.Drawing.Bitmap CreatePhaseBitmap()
        {
            ComplexPlainAnalysis anal1 = CurrentAnalisys() as ComplexPlainAnalysis;
            ComplexPlainSolver sol1 = anal1.Solver as ComplexPlainSolver;
            NodeSingle node = null;
            foreach (var item in CurrentCircuit.Nodes.Keys)
            {
                if (item == "out")
                {
                    node = CurrentCircuit.Nodes[item];
                    break;
                }
            }
            if (node == null) return null;
            System.Drawing.Bitmap bmp = sol1.TakeSnapShot(node, anal1);// CreateBitmap();
            FaseBitmap = ImageUtils.Bitmap2BitmapSource(bmp);
            return bmp;
        }

        public override void SaveBitmap(object obj)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Net List files (*.bmp)|*.bmp|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                Viewport3DHelper.SaveBitmap(ViewPort, save.FileName);
                //base.Export(obj);
            }
        }

        public override void CopyTo(object obj)
        {
            Viewport3DHelper.Copy(ViewPort);
        }

    }
}
