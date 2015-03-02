using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
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
    /// Lógica de interacción para Plain2DAnalizer.xaml
    /// </summary>
    public partial class Plain2DAnalizer : Window
    {
        ObservableDataSource<Tuple<double, double>> sourcecurrent = null;
        ObservableDataSource<Tuple<double, double>> sourcevoltage = null;

        public Plain2DAnalizer()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create first source
            sourcecurrent = new ObservableDataSource<Tuple<double, double>>();
            sourcecurrent.SetXYMapping(z =>
            {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });
            sourcevoltage = new ObservableDataSource<Tuple<double, double>>();
            sourcevoltage.SetXYMapping(z =>
            {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });

            List<Point> list = new List<Point>();

            Random rnd = new Random();
            for (int i = 0; i < 300; i++)
            {
                sourcecurrent.Collection.Add(new Tuple<double, double>(i, rnd.Next(100)));
                sourcevoltage.Collection.Add(new Tuple<double, double>(i, 10 + rnd.Next(20)));
                list.Add(new Point(i, rnd.Next(50)));
            }
            //LineGraph line = new LineGraph(source1);
            //line.AddToPlotter(plotter);
            // plotter.Children.Add(line);

            linegraph.DataSource = sourcecurrent;
            linephase.DataSource = sourcevoltage;
            //axis = new VerticalAxisTitle();
            //linegraph = new Microsoft.Research.DynamicDataDisplay.LineGraph(
            // Creating the new DraggablePoint
            int x1 = 10, y1 = 10;
            Point q = new Point(x1, y1);
            var globalPoint = new DraggablePoint(q);

            globalPoint.PositionChanged += (s, r) =>
            {
                globalPoint.Position = q;
            };

            // Set the point on the map
            plotter.Children.Add(globalPoint);

           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Point> list = new List<Point>();

            Random rnd = new Random();
            for (int i = 0; i < 300; i++)
            {
                list.Add(new Point(i, rnd.Next(50)));
            }
            EnumerableDataSource<Point> edc;
            edc = new EnumerableDataSource<Point>(list);
            edc.SetXMapping(x => x.X);
            edc.SetYMapping(y => Convert.ToDouble(y.Y));
            edc.AddMapping(CircleElementPointMarker.ToolTipTextProperty, s => String.Format("Y-Data : {0}\nX-Data : {1}", s.Y, s.X));

            LineGraph line =new LineGraph(edc);
            innerPlotter.Children.Add(line);
            //,
            line.LinePen = new Pen(Brushes.Transparent, 3);
            //line. new CircleElementPointMarker
            //{
            //    Size = 15,
            //    Brush = border,
            //    Fill = c2
            //};
            //                null
            //                ));
        }
    }
}
