using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CircuitDesigner
{
    public class DrawerCanvas:Canvas
    {


        public enum DrawingForms { none, line, rectangle, circle }

        public IList<DrawingForms> UserTypes
        {
            get
            {
                // Will result in a list like {"Tester", "Engineer"}
                return Enum.GetValues(typeof(DrawingForms)).Cast<DrawingForms>().ToList<DrawingForms>();
            }
        }


        public static readonly DependencyProperty DrawingFormProperty =
                DependencyProperty.Register("DrawingForm", typeof(DrawingForms), typeof(DrawerCanvas));

        Point startpoint, endpoint;
        private Point start;
        private bool isDragging;
        private Shape movedElement;
        private int currentZ;

        public DrawingForms DrawingForm
        {
            get { return (DrawingForms)GetValue(DrawingFormProperty); }
            set { SetValue(DrawingFormProperty, value); }
        }




        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            
            var canvas = this;

            if (canvas.CaptureMouse())
            {
                startpoint = e.GetPosition(canvas);
                Shape shape = null;
                switch (DrawingForm)
                {
                    case DrawingForms.none:

                        break;
                    case DrawingForms.line:
                        var line = new Line
                        {
                            Stroke = Brushes.Blue,
                            StrokeThickness = 3,
                            X1 = startpoint.X,
                            Y1 = startpoint.Y,
                            X2 = startpoint.X,
                            Y2 = startpoint.Y,
                        };
                        //line.MouseDown += Line_MouseDown;
                        Canvas.SetLeft(line, 0);
                        Canvas.SetTop(line, 0);
                        shape = line;
                        break;
                    case DrawingForms.circle:
                        Ellipse ellipse = new Ellipse
                        {
                            Stroke = Brushes.Blue,
                            StrokeThickness = 3,
                        };
                        Canvas.SetLeft(ellipse, startpoint.X);
                        Canvas.SetTop(ellipse, startpoint.Y);
                        shape = ellipse;
                        break;
                    case DrawingForms.rectangle:
                        Rectangle rectangle = new Rectangle
                        {
                            Stroke = Brushes.Blue,
                            StrokeThickness = 3,
                        };
                        Canvas.SetLeft(rectangle, startpoint.X);
                        Canvas.SetTop(rectangle, startpoint.Y);
                        shape = rectangle;
                        break;
                        //default:
                }

                if (shape != null)
                    canvas.Children.Add(shape);
            }
        }

        //private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    Line line = sender as Line;


        //}


        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (DrawingForm != DrawingForms.none)
                return;
            
            //Check if the click was in a chape
            if (e.Source is Shape)
            {
                // Get the mouse position
                start = e.GetPosition(this);
                // Initialize some components and set opacity to 50%
                isDragging = true;
                movedElement = (Shape)e.Source;
                ((Shape)e.Source).Opacity = 0.5;
                CaptureMouse();
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (DrawingForm != DrawingForms.none)
                return;

            if (movedElement == null)
                return;

            if (isDragging)
            {
                Point Pt = e.GetPosition(this);
                // Get the actual position of the Shape
                double CurrentLeft = (double)movedElement.GetValue(Canvas.LeftProperty);
                double CurrentTop = (double)movedElement.GetValue(Canvas.TopProperty);

                // Calc the new position
                double newLeft = CurrentLeft + Pt.X - start.X;
                double newTop = CurrentTop + Pt.Y - start.Y;

                // Move the element
                movedElement.SetValue(Canvas.LeftProperty, newLeft);
                movedElement.SetValue(Canvas.TopProperty, newTop);

                start = Pt;
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (DrawingForm != DrawingForms.none)
                return;

            if (movedElement == null)
                return; 

           
            // Restore the values
            movedElement.Opacity = 1;
            movedElement.SetValue(Canvas.ZIndexProperty, ++currentZ);
            isDragging = false;
            ReleaseMouseCapture();
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            var canvas = this;//(Canvas)sender;

            if (canvas.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                endpoint = e.GetPosition(canvas);

                switch (DrawingForm)
                {
                    case DrawingForms.none:

                        break;
                    case DrawingForms.line:
                        var line = canvas.Children.OfType<Line>().LastOrDefault();

                        if (line != null)
                        {
                            line.X2 = endpoint.X;
                            line.Y2 = endpoint.Y;
                        }
                        break;
                    case DrawingForms.circle:
                        var ellipse = canvas.Children.OfType<Ellipse>().LastOrDefault();

                        if (ellipse != null)
                        {
                            //var endPoint = e.GetPosition(canvas);
                            //Math.mo
                            double x = startpoint.X - endpoint.X;
                            double y = startpoint.Y - endpoint.Y;
                            double rad = Math.Sqrt(x * x + y * y);
                            ellipse.Width = 2 * rad;
                            ellipse.Height = 2 * rad;
                            Canvas.SetLeft(ellipse, startpoint.X - rad);
                            Canvas.SetTop(ellipse, startpoint.Y - rad);
                        }                    
                        break;
                    case DrawingForms.rectangle:
                        var rectangle = canvas.Children.OfType<Rectangle>().LastOrDefault();

                        if (rectangle != null)
                        {
                            //var endPoint = e.GetPosition(canvas);
                            //Math.mo
                            double x = startpoint.X - endpoint.X;
                            double y = startpoint.Y - endpoint.Y;
                            double rad = Math.Sqrt(x * x + y * y);
                            rectangle.Width = 2 * rad;
                            rectangle.Height = 2 * rad;
                            Canvas.SetLeft(rectangle, startpoint.X - rad);
                            Canvas.SetTop(rectangle, startpoint.Y - rad);
                        }
                        break;

                        //default:
                }

                
            }
        }


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        
            this.ReleaseMouseCapture();
        }

    }
}
