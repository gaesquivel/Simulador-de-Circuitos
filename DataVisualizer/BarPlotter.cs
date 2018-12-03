using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DataVisualizer
{
    public class BarPlotter : PointsGraphBase
    {
        public int BarWidth { get; private set; }
        public Brush Fill { get; private set; }
        public Brush Stroke { get; private set; }
        public double StrokeThickness { get; private set; }


        public BarPlotter()
        {
            BarWidth = 2;
            Fill = Brushes.Red;
            Stroke = Brushes.Orange;
            StrokeThickness = 2;
        }


        protected override void OnRenderCore(System.Windows.Media.DrawingContext dc, RenderState state)
        {
            if (DataSource == null) return;
            var transform = Plotter2D.Viewport.Transform;

            DataRect bounds = DataRect.Empty;
            using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
            {
                Point point = new Point();
                while (enumerator.MoveNext())
                {
                    enumerator.GetCurrent(ref point);
                    enumerator.ApplyMappings(this);

                    Point zero = new Point(point.X, 0);
                    Point screenPoint = point.DataToScreen(transform);
                    Point screenZero = zero.DataToScreen(transform);

                    double height = screenPoint.Y - screenZero.Y;

                    if (height >= 0)
                    {
                        dc.DrawRectangle(Fill, new Pen(Stroke, StrokeThickness)
                                , new Rect(screenPoint.X - BarWidth / 2, screenZero.Y, BarWidth, height));
                    }
                    else
                    {
                        dc.DrawRectangle(Fill, new Pen(Stroke, StrokeThickness),
                                new Rect(screenPoint.X - BarWidth / 2, screenPoint.Y, BarWidth, -height));
                    }

                    bounds = DataRect.Union(bounds, point);

                }
            }

            Viewport2D.SetContentBounds(this, bounds);
        }

    }
}
