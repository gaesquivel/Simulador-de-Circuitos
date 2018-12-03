using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using ElectricalAnalysis;

namespace DataVisualizer.MVVM.ViewModel
{
    // http://reference.wolfram.com/mathematica/tutorial/ThreeDimensionalSurfacePlots.html

    public enum ColorCoding
    {
        /// <summary>
        /// No color coding, use coloured lights
        /// </summary>
        ByLights,

        /// <summary>
        /// Color code by gradient in y-direction using a gradient brush with white ambient light
        /// </summary>
        ByGradientY,
        ModuleLevel,
        ModuleBlue,
        ModuleFire,
        Fase1, Fase2, Fase3,
        Phaselevel,
        PhaseFire,
        PhaseBlue
    }


    public class SurfacePlotVisual3D : ModelVisual3D
    {

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof (Point3D[,]), typeof (SurfacePlotVisual3D),
                                        new UIPropertyMetadata(null, ModelChanged));

        public static readonly DependencyProperty ColorValuesProperty =
            DependencyProperty.Register("ColorValues", typeof (double[,]), typeof (SurfacePlotVisual3D),
                                        new UIPropertyMetadata(null, ModelChanged));

        public static readonly DependencyProperty SurfaceBrushProperty =
            DependencyProperty.Register("SurfaceBrush", typeof (Brush), typeof (SurfacePlotVisual3D),
                                        new UIPropertyMetadata(null, ModelChanged));

        //public static readonly DependencyProperty ColorCodingProperty =
        //    DependencyProperty.Register("ColorCoding", typeof(ColorCoding), 
        //                        typeof(SurfacePlotVisual3D),
        //                        new UIPropertyMetadata(null, ModelChanged));


        private readonly ModelVisual3D visualChild;

        public SurfacePlotVisual3D()
        {
            IntervalX = 1;
            IntervalY = 1;
            IntervalZ = 0.25;
            //ScaleX = 1;
            //ScaleY = 1;
            //ScaleZ = 1;
            FontSize = 0.3; //0.06
            LineThickness = 0.04; //0.01
            AutoScale = true;
            SubAxisCount = 4;
            SurfaceBrush = Brushes.Aqua;

            visualChild = new ModelVisual3D();
            Children.Add(visualChild);
        }

        public ColorCoding ColorCoding
        {
            get; set;
            //get { return (ColorCoding)GetValue(ColorCodingProperty); }
            //set { SetValue(ColorCodingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the points defining the surface.
        /// </summary>
        public Point3D[,] Points
        {
            get { return (Point3D[,]) GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color values corresponding to the Points array.
        /// The color values are used as Texture coordinates for the surface.
        /// Remember to set the SurfaceBrush, e.g. by using the BrushHelper.CreateGradientBrush method.
        /// If this property is not set, the z-value of the Points will be used as color value.
        /// </summary>
        public double[,] ColorValues
        {
            get { return (double[,]) GetValue(ColorValuesProperty); }
            set {
                SetValue(ColorValuesProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the brush used for the surface.
        /// </summary>
        public Brush SurfaceBrush
        {
            get { return (Brush) GetValue(SurfaceBrushProperty); }
            set { SetValue(SurfaceBrushProperty, value); }
        }

        public Point4D[,] OriginalData { get; set; }

        // todo: make Dependency properties
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double ScaleZ { get; set; }
        public int SubAxisCount { get; set; }
        public bool AutoScale { get; set; }
        public double IntervalX { get; set; }
        public double IntervalY { get; set; }
        public double IntervalZ { get; set; }
        public double FontSize { get; set; }
        public double LineThickness { get; set; }

        private static void ModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfacePlotVisual3D) d).UpdateModel();
        }

        private void UpdateModel()
        {
            if (visualChild == null)
                return;
            visualChild.Content = CreateModel();
        }

        private Model3D CreateModel()
        {
            var plotModel = new Model3DGroup();
            if (Points == null)
                return plotModel;

            int rows = Points.GetUpperBound(0) + 1;
            int columns = Points.GetUpperBound(1) + 1;
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

        
            #region Color things

            double minColorValue = double.MaxValue;
            double maxColorValue = double.MinValue;

            //busqueda de maximos y minimos
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    double x = Points[i, j].X;
                    double y = Points[i, j].Y;
                    double z = Points[i, j].Z;
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                    maxZ = Math.Max(maxZ, z);
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    minZ = Math.Min(minZ, z);
                    if (ColorValues != null)
                    {
                        maxColorValue = Math.Max(maxColorValue, ColorValues[i, j]);
                        minColorValue = Math.Min(minColorValue, ColorValues[i, j]);
                    }
                }

            // make color value 0 at texture coordinate 0.5
            if (Math.Abs(minColorValue) < Math.Abs(maxColorValue))
                minColorValue = -maxColorValue;
            else
                maxColorValue = -minColorValue;

            #endregion


            #region Texture section

            // set the texture coordinates by z-value or ColorValue
            var texcoords = new Point[rows,columns];
            if (OriginalData != null && ColorCoding >= ColorCoding.Fase1)
            {
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < columns; j++)
                    {
                        double u = MathUtil.Scale(minZ, maxZ, OriginalData[i, j].Z, 0.5);
                        double v = OriginalData[i, j].W;
                        double uu = 0.5 + u * Math.Cos(v);
                        double vv = 0.5 + u * Math.Sin(v);
                        texcoords[i, j] = new Point(uu, vv);
                    }
            }
            else
            {
                
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < columns; j++)
                    {
                        //u esta escalado
                        double u = (Points[i, j].Z - minZ) / (maxZ - minZ);
                    //double v = 
                        if (ColorValues != null)
                            if (maxZ - minZ > 0)
                                u = (ColorValues[i, j] - minColorValue) / (maxColorValue - minColorValue);
                            else
                                u = 1;
                        texcoords[i, j] = new Point(u, u);
                    }
                
            }

            if (AutoScale)
            {
                ScaleX = 10 / Math.Abs(maxX - minX);
                ScaleY = 10 / Math.Abs(maxY - minY);
                if (maxZ != minZ)
                    ScaleZ = 5 / Math.Abs(maxZ - minZ);
                else
                    ScaleZ = 10;
            }

            var surfaceMeshBuilder = new MeshBuilder();
            surfaceMeshBuilder.AddRectangularMesh(Points, texcoords);
            surfaceMeshBuilder.Scale(ScaleX, ScaleY, ScaleZ);

            var surfaceModel = new GeometryModel3D(surfaceMeshBuilder.ToMesh(),
                                                   MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1, 0));
            surfaceModel.BackMaterial = surfaceModel.Material;

            #endregion

            maxX *= ScaleX;
            minX *= ScaleX;
            maxY *= ScaleY;
            minY *= ScaleY;
            maxZ *= ScaleZ;
            minZ *= ScaleZ;

            IntervalX = (maxX - minX) / SubAxisCount;
            IntervalY = (maxY - minY) / SubAxisCount;
            IntervalZ = (maxZ - minZ) / SubAxisCount;
            
            #region eje x

            var axesMeshBuilder = new MeshBuilder();
            for (double x = minX; x <= maxX; x += IntervalX)
            {
                double j = (x - minX)/(maxX - minX)*(columns - 1);
                //var path = new List<Point3D> { new Point3D(x , minY , minZ) };
                var path = new List<Point3D> { new Point3D(x, minY , minZ) };
                for (int i = 0; i < rows; i++)
                {
                    Point3D p = BilinearInterpolation(Points, i, j);
                    p.X *= ScaleX;
                    p.Y *= ScaleY;
                    p.Z *= ScaleZ;
                    path.Add(p);
                }
                path.Add(new Point3D(x, maxY, minZ));

                axesMeshBuilder.AddTube(path, LineThickness, 9, false);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(StringUtils.CodeString(x / ScaleX), Brushes.Black, true, FontSize,
                                                                           new Point3D(x, minY - FontSize * 2, minZ),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }

            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D("σ axis", Brushes.Black, true, FontSize,
                                                                           new Point3D((minX + maxX)*0.5,
                                                                                       minY - FontSize * 4, minZ),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }

            #endregion


            #region eje y

            for (double y = minY; y <= maxY; y += IntervalY)
            {
                double i = (y - minY)/(maxY - minY)*(rows - 1);
                var path = new List<Point3D> {new Point3D(minX, y, minZ)};
                for (int j = 0; j < columns; j++)
                {
                    Point3D p = BilinearInterpolation(Points, i, j);
                    p.X *= ScaleX;
                    p.Y *= ScaleY;
                    p.Z *= ScaleZ;
                    path.Add(p);
                }
                path.Add(new Point3D(maxX, y, minZ));

                axesMeshBuilder.AddTube(path, LineThickness, 9, false);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(StringUtils.CodeString(y / ScaleY), Brushes.Black, true, FontSize,
                                                                           new Point3D(minX - FontSize * 2, y, minZ),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D("ω axis", Brushes.Black, true, FontSize,
                                                                           new Point3D(minX - FontSize * 4,
                                                                                       (minY + maxY) * 0.5, minZ),
                                                                           new Vector3D(0, 1, 0), new Vector3D(-1, 0, 0));
                plotModel.Children.Add(label);
            }

            #endregion


            #region eje z

            double z0 = 0;
            if (maxZ != minZ)
            {
                z0 = (int)(minZ / IntervalZ) * IntervalZ;

                for (double z = z0; z <= maxZ + double.Epsilon; z += IntervalZ)
                {
                    GeometryModel3D label = TextCreator.CreateTextLabelModel3D(StringUtils.CodeString(z / ScaleZ), Brushes.Black, true, FontSize,
                                                                               new Point3D(minX - FontSize * 2, maxY, z),
                                                                               new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
                    plotModel.Children.Add(label);
                }
                {
                    GeometryModel3D label = TextCreator.CreateTextLabelModel3D("|Z| axis", Brushes.Black, true, FontSize,
                                                                               new Point3D(minX - FontSize * 8, maxY,
                                                                                           (minZ + maxZ) * 0.5),
                                                                               new Vector3D(0, 0, 1), new Vector3D(1, 0, 0));
                    plotModel.Children.Add(label);
                }
            }
            #endregion

            //la base del plano
            var bb = new Rect3D(minX, minY, minZ, maxX - minX, maxY - minY, 0 * (maxZ - minZ));
            axesMeshBuilder.AddBoundingBox(bb, LineThickness);

            var axesModel = new GeometryModel3D(axesMeshBuilder.ToMesh(), Materials.Black);

            plotModel.Children.Add(surfaceModel);
            plotModel.Children.Add(axesModel);
            //this.


            return plotModel;
        }

        private static Point3D BilinearInterpolation(Point3D[,] p, double i, double j)
        {
            int n = p.GetUpperBound(0);
            int m = p.GetUpperBound(1);
            var i0 = (int) i;
            var j0 = (int) j;
            if (i0 + 1 >= n) i0 = n - 2;
            if (j0 + 1 >= m) j0 = m - 2;

            if (i < 0) i = 0;
            if (j < 0) j = 0;
            double u = i - i0;
            double v = j - j0;
            Vector3D v00 = p[i0, j0].ToVector3D();
            Vector3D v01 = p[i0, j0 + 1].ToVector3D();
            Vector3D v10 = p[i0 + 1, j0].ToVector3D();
            Vector3D v11 = p[i0 + 1, j0 + 1].ToVector3D();
            Vector3D v0 = v00*(1 - u) + v10*u;
            Vector3D v1 = v01*(1 - u) + v11*u;
            return (v0*(1 - v) + v1*v).ToPoint3D();
        }
    }
}