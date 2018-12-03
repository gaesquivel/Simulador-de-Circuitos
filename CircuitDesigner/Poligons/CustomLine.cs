using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;

namespace CircuitDesigner.Poligons
{
    public class CustomLine : FrameworkElement, INotifyPropertyChanged
    {

        // Create a collection of child visual objects.

        private VisualCollection _children;
        public SelectionType Selection = SelectionType.NotSelected;
        public enum SelectionType : int
        {
            LineSelected,
            PointSelection,
            NotSelected
        }

        #region "Constructor"

        public CustomLine(PointCollection p_position)
        {
            Points = p_position;

            _children = new VisualCollection(this);

            for (int i = 0; i <= Points.Count - 1; i++)
            {
                if (i < Points.Count - 1)
                {
                    _children.Add(CreateDrawingVisualLine(Points[i], Points[i + 1]));
                    _children.Add(CreateInvisibleDrawingVisualLine(Points[i], Points[i + 1]));
                }
                CanvasPoint temp = new CanvasPoint(Points[i]);
                temp.Opacity = 0;
                _children.Add(temp);
            }

            // Add the event handler for MouseLeftButtonDown.
            PreviewMouseLeftButtonDown += CustomLineMouseDown;
        }
        #endregion

        #region "Properties"

        private bool _HitFromPoint = false;
        public bool HitFromPoint
        {
            get { return _HitFromPoint; }
            set { _HitFromPoint = value; }
        }

        private bool _Multiselect = false;
        public bool Multiselect
        {
            get { return _Multiselect; }
            set { _Multiselect = value; }
        }

        private PointCollection p_LinePoints = new PointCollection();
        public PointCollection Points
        {
            get { return p_LinePoints; }
            set
            {
                p_LinePoints = value;
                INotifyChange("PositionOnCanvas");
            }
        }

        private int _SelectedPointID;
        public int SelectedPointID
        {
            get { return _SelectedPointID; }
            set { _SelectedPointID = value; }
        }

        public void INotifyChange(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


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
        #endregion

        #region "Drawing"
        private DrawingVisual CreateDrawingVisualLine(Point p1, Point p2)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            Pen pen = default(Pen);
            //(Brush, 70)

            if (Selection == SelectionType.LineSelected)
            {
                pen = new Pen(Brushes.Black, 1);
            }
            else
            {
                pen = new Pen(Brushes.Gray, 1);
            }
            pen.EndLineCap = PenLineCap.Round;
            pen.DashCap = PenLineCap.Round;
            pen.LineJoin = PenLineJoin.Round;
            pen.StartLineCap = PenLineCap.Round;
            pen.MiterLimit = 10.0;
            drawingContext.DrawLine(pen, p1, p2);
            // Persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        private DrawingVisual CreateInvisibleDrawingVisualLine(Point p1, Point p2)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawLine(new Pen(Brushes.Transparent, 3), p1, p2);

            // Persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        public void ReDraw()
        {
            _children.Clear();
            _children = new VisualCollection(this);

            for (int i = 0; i <= Points.Count - 1; i++)
            {
                if (i < Points.Count - 1)
                {
                    _children.Add(CreateDrawingVisualLine(Points[i], Points[i + 1]));
                    _children.Add(CreateInvisibleDrawingVisualLine(Points[i], Points[i + 1]));
                }
                if (Selection == SelectionType.PointSelection)
                {
                    _children.Add(new CanvasPoint(Points[i]));
                }

            }
        }
        #endregion

        public void CustomLineMouseDown(object sender, MouseEventArgs e)
        {
            if (!(e.OriginalSource is CanvasPoint))
            {
                HitFromPoint = false;
                if (Selection == SelectionType.NotSelected)
                {
                    Selection = SelectionType.LineSelected;
                    for (int i = 0; i <= _children.Count - 1; i++)
                    {
                        if ((_children[i]) is DrawingVisual)
                        {
                            ((DrawingVisual)_children[i]).Opacity = 0.5;
                        }
                        else if ((_children[i]) is CanvasPoint)
                        {
                            ((CanvasPoint)_children[i]).Opacity = 0.5;
                        }
                    }
                    ReDraw();
                }
                else if (Selection == SelectionType.LineSelected)
                {
                    Selection = SelectionType.PointSelection;
                    ReDraw();
                }
                else
                {
                    Selection = SelectionType.NotSelected;
                    ReDraw();
                }
            }
            else if ((e.OriginalSource) is CanvasPoint)
            {
                HitFromPoint = true;
                CanvasPoint CurrentSelectedPoint = default(CanvasPoint);
                CurrentSelectedPoint = (CanvasPoint)e.OriginalSource;
                CurrentSelectedPoint.Opacity = 0.5;
                for (int i = 0; i <= this._children.Count - 1; i++)
                {
                    if ((this._children[i]) is CanvasPoint)
                    {
                        if (!Multiselect)
                        {
                            CanvasPoint po = (CanvasPoint)this._children[i];
                            if (object.ReferenceEquals(po, CurrentSelectedPoint))
                            {
                                po.Opacity = 1;
                            }
                            else
                            {
                                po.Opacity = 0.5;
                            }
                        }
                    }
                }
                for (int i = 0; i <= Points.Count - 1; i++)
                {
                    if (CurrentSelectedPoint.PositionOnCanvas == (Points[i]))
                    {
                        SelectedPointID = i;
                    }
                }
            }
        }

    }

}
