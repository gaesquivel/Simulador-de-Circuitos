using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Input;
using WPFExtensions.Controls;
using DiagramDesigner;
using System.Windows.Controls;

namespace CircuitDesigner.Poligons
{

    public class CanvasPoint : FrameworkElement, INotifyPropertyChanged
    {

        Point Origin;
        bool isMoved = false;

        // Create a collection of child visual objects.
        private VisualCollection _children;
        #region "Constructors"
        public CanvasPoint()
        {
            _children = new VisualCollection(this);
            _children.Add(CreateDrawingVisualCircle());

        }

        public CanvasPoint(Point p_position)
        {
            PositionOnCanvas = p_position;

            _children = new VisualCollection(this);
            _children.Add(CreateDrawingVisualCircle());
        }
        #endregion

        #region "Properties"

        private Point p_PositionOnCanvas = new Point();
        public Point PositionOnCanvas
        {
            get { return p_PositionOnCanvas; }
            set
            {
                p_PositionOnCanvas = value;
                INotifyChange("PositionOnCanvas");
            }
        }
        #endregion

        #region "Overided properties"

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.LeftButton == MouseButtonState.Released)
                isMoved = false;

            if (isMoved)
                return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isMoved = true;
                Origin = PositionOnCanvas;
            }
            //if (e.LeftButton == MouseButtonState.Pressed)
            //    isMoved = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!isMoved || e.LeftButton != MouseButtonState.Pressed)
                return;

            Canvas drw = GetDesignerCanvas(this);
            if (drw != null)
            {
                PositionOnCanvas =
                            new Point(PositionOnCanvas.X + 10, PositionOnCanvas.Y + 10);

                InvalidateVisual();
                //drw.UpdateLayout();
                //DrawingContext
            }
            e.Handled = true;
            //ZoomControl.Instance.MousePosition; // Mouse.GetPosition(this);
                //Mouse.GetPosition(drw);

                //e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            isMoved = false;
        }

        private DesignerCanvas GetDesignerCanvas(DependencyObject element)
        {
            while (element != null && !(element is DesignerCanvas))
                element = VisualTreeHelper.GetParent(element);

            return element as DesignerCanvas;
        }


        #endregion

        #region "Drawing"
        // Create a DrawingVisual that contains a rectangle.
        private DrawingVisual CreateDrawingVisualCircle()
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // Create a circle and draw it in the DrawingContext.
            drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 1.0), PositionOnCanvas, 4, 4);

            // Persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        #endregion

        #region "Events"
        public void INotifyChange(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }

}
