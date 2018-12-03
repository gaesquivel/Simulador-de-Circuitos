using CircuitMVVMBase.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using CircuitDesigner.Poligons;
using CircuitMVVMBase.Commands;
using CircuitMVVMBase.Collections;

namespace CircuitDesigner.MVVM.ViewModel
{
    public class CircuitDesignerViewModel:ViewModelBase
    {

        #region Data Members

        /// <summary>
        /// The singleton instance.
        /// This is a singleton for convenience.
        /// </summary>
        static CircuitDesignerViewModel instance = new CircuitDesignerViewModel();

        public static CircuitDesignerViewModel Instance { get { return instance; } }

        /// <summary>
        /// The list of rectangles that is displayed both in the main window and in the overview window.
        /// </summary>
        public NotifyObservableCollection<INotifyPropertyChanged> Shapes { get; protected set; }

        RelayCommand drawlinecommand;
        public RelayCommand DrawLineCommand
        {
            get { return drawlinecommand ?? (drawlinecommand = new RelayCommand((Action<object>)DrawLine)); }
        }

        protected Canvas drawingsurface;
        public Canvas DrawingSurface
        {
            get { return drawingsurface; }
            set
            {
                if (RaisePropertyChanged(value, ref drawingsurface))
                    drawingsurface.PreviewMouseDown += Drawingsurface_MouseDown;
            }
        }

        


        #endregion Data Members




        public CircuitDesignerViewModel()
        {
            Shapes = new NotifyObservableCollection<INotifyPropertyChanged>();
            //Shapes.CollectionChanged += Shapes_CollectionChanged;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            OpenCircuit();
        }

        //private void Shapes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    FrameworkElement f = e.NewItems[0] as FrameworkElement;
        //    f.ch

        //}

        private void Drawingsurface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }


        private void DrawLine(object obj)
        {
            
        }


        public void OpenCircuit()
        {
            Shapes.Clear();
            //StreamReader reader = File.OpenText("Circuits/test1.asc");
            StreamReader reader = File.OpenText("Circuits/svdt-mos2.asc");
            string cir = reader.ReadToEnd();
            reader.Close();

            string[] lines = cir.Replace('\t', ' ').Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); ;

            CanvasPoint a = new CanvasPoint(new Point(0, 0));
            a.Focus();
            Shapes.Add(a);

            foreach (var line in lines)
            {
                string[] rect = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                List<int> rects = new List<int>();
                //WIRE 384 -192 192 -192
                if (line.StartsWith("WIRE"))
                {
                    Debug.WriteLine(line);
                    for (int i = 1; i < rect.Length; i++)
                    {
                        int o = 0;
                        if (int.TryParse(rect[i], out o))
                        {
                            rects.Add(o);
                        }
                    }
                    //Line lin = new Line()
                    //{
                    //    X1 = rects[0],
                    //    X2 = rects[2],
                    //    Y1 = rects[1],
                    //    Y2 = rects[3],
                    //    Stroke = Brushes.Black,
                    //    StrokeThickness = 2
                    //};
                    PointCollection p = new PointCollection();
                    p.Add(new Point(rects[0], rects[1]));
                    p.Add(new Point(rects[2], rects[3]));
                    CustomLine lin = new CustomLine(p);

                    Shapes.Add(lin);
                }
                //SYMBOL nmos 144 128 R0
                //SYMATTR InstName M1
                //SYMATTR Value IRFP450
                //SYMATTR Prefix X
                if (line.StartsWith("SYMBOL"))
                {
                    for (int i = 2; i < 4; i++)
                    {
                        int o = 0;
                        if (int.TryParse(rect[i], out o))
                        {
                            rects.Add(o);
                        }
                    }
                    //Ellipse lin = new Ellipse()
                    //{
                    //    Margin = new Thickness(rects[0], rects[1], 0, 0),
                    //    Width = 100,
                    //    Height = 100,
                    //    Stroke = Brushes.Blue,
                    //    StrokeThickness = 2

                    //};

                    //Shapes.Add(lin);
                }

            }
            //Line lin2 = new Line()
            //{
            //    X1 = 10,
            //    X2 = 20,
            //    Y1 = 10,
            //    Y2 = 20,
            //    Width = 2,
            //    Stroke = Brushes.Red
            //};
            //Shapes.Add(lin2);
            //TextBox txt = new TextBox();
            //txt.Width = 100;
            //txt.Height = 100;
            ////txt.Margin=
            //txt.Text = "Hola";
            //Shapes.Add(txt);
        }
    }
}
