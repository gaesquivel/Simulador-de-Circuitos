using ElectricalAnalysis;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
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
            //plotter.DataTransform = new Log10Transform();
            
            // Create first source
            sourcecurrent = new ObservableDataSource<Tuple<double, double>>();
            sourcecurrent.SetXYMapping(z =>
            {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });

            //plotter.DataTransform = new Log10Transform();
            //HorizontalAxis xAxis = new HorizontalAxis
            //{
            //    TicksProvider = new NumericTicksProvider(),
            //    LabelProvider = new ExponentialLabelProvider()
            //};
            //plotter.MainHorizontalAxis = xAxis;
            //VerticalAxis yAxis = new VerticalAxis
            //{
            //    //TicksProvider = new LogarithmNumericTicksProvider(10),
            //    TicksProvider = new NumericTicksProvider(),
            //    LabelProvider = new ExponentialLabelProvider()
            //};
            //plotter.MainVerticalAxis = yAxis;

            HorizontalAxis axis = (HorizontalAxis)plotter.MainHorizontalAxis;
            //axis.LabelProvider.SetCustomFormatter(info => info.Tick.ToString("#.######E+0"));
            axis.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            VerticalAxis axisv = (VerticalAxis)plotter.MainVerticalAxis;
            //String.Format(new TelephoneFormatter(), "{0}", 0)
            axisv.LabelProvider.SetCustomFormatter(info => info.Tick.ToString("#.######E+0"));

            sourcevoltage = new ObservableDataSource<Tuple<double, double>>();
            sourcevoltage.SetXYMapping(z =>
            {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });

            //otherPlotter.DataTransform = new Log10Transform();
            //xAxis = new HorizontalAxis
            //{
            //    TicksProvider = new LogarithmNumericTicksProvider(10),
            //    LabelProvider = new UnroundingLabelProvider()
            //};
            //otherPlotter.MainHorizontalAxis = xAxis;
            //yAxis = new VerticalAxis
            //{
            //    TicksProvider = new LogarithmNumericTicksProvider(10),
            //    LabelProvider = new UnroundingLabelProvider()
            //};
            //otherPlotter.MainVerticalAxis = yAxis;



            //List<Point> list = new List<Point>();

            Random rnd = new Random();
            for (int i = 10; i < 300; i++)
            {
                sourcecurrent.Collection.Add(new Tuple<double, double>(i, 20000 + rnd.Next(10000)));
                sourcevoltage.Collection.Add(new Tuple<double, double>(i, 30 + rnd.Next(20)));
                //list.Add(new Point(i, rnd.Next(50)));
            }
            //plotter1.VerticalAxis.LabelProvider = new ExponentialLabelProvider();
            //LineGraph line = new LineGraph(source1);
            //line.AddToPlotter(plotter);
            // plotter.Children.Add(line);

            //otherPlotter.MainVerticalAxis.

            linegraph.DataSource = sourcevoltage ;
            linephase.DataSource = sourcecurrent;
            //axis = new VerticalAxisTitle();
            //linegraph = new Microsoft.Research.DynamicDataDisplay.LineGraph(
            // Creating the new DraggablePoint
            if (false)
            {
                int x1 = 10, y1 = 10;
                Point q = new Point(x1, y1);
                var globalPoint = new DraggablePoint(q);

                globalPoint.PositionChanged += (s, r) =>
                {
                    globalPoint.Position = q;
                };
                plotter.Children.Add(globalPoint);

            }
            
            // Set the point on the map

           
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
            plotter.Children.Add(line);
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
