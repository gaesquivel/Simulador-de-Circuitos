using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using MathNet.Numerics.Providers.FourierTransform;
using System.ComponentModel;
using MathNet.Numerics;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace DataVisualizer.MVVM.ViewModel
{
    public class FourierViewModel : TransformViewModelBase
    {
        public class FFTWindow
        {
            public enum WindowType { Rectangular, Triangular, Hamming}

            WindowType windowtype;

            public WindowType SampleType
            {
                get { return windowtype; }
                set
                {
                    windowtype = value;

                }
            }


            /// <summary>
            /// Number of total samples for FFT
            /// </summary>
            public int NumberOfSamples { get; set; }


            private int numberofpoint;
            /// <summary>
            /// number of point of window
            /// Must be power of 2: 32, 64, etc
            /// </summary>
            [DefaultValue(64)]
            public int NumberOfPoints
            {
                get {
                    //if (NumberOfSamples == 0)
                    return numberofpoint;
                }
                set {
                    if (value >= 0)
                    {
                        numberofpoint = PreviousPowerOf2(value);
                    }
                }

            }

            private int startat;
            /// <summary>
            /// Where the window count start
            /// </summary>
            public int StartAt {
                get { return startat; }
                set {
                    if (value <= 0)
                        throw new Exception("number Must be positive");
                    if (value < NumberOfSamples - NumberOfPoints)
                    {
                        startat = value;
                    }
                }
            }

            /// <summary>
            /// Where the window count finish
            /// </summary>
            public int FinishAt { get { return NumberOfPoints + StartAt; } }

            /// <summary>
            /// Temporal function to be TRANSFORM for FFT
            /// </summary>
            public List<double> TemporalFunction { get; protected set; }

            /// <summary>
            /// Temporal Sampled and Windowed function for FFT
            /// </summary>
            public List<double> SampledFunction { get; protected set; }

            /// <summary>
            /// Output result from FFT TRANSFORM
            /// </summary>
            public List<Complex32> FFTOutput { get; protected set; }


            public FFTWindow()
            {
                TemporalFunction = new List<double>();
                SampledFunction = new List<double>();
                FFTOutput = new List<Complex32>();
            }

            /// <summary>
            /// Calculate the previous power of 2 of value
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static int PreviousPowerOf2(int value)
            {
                if (value <= 0)
                    throw new Exception("number Must be positive");
                return (int)Math.Pow(2, (int)Math.Floor(Math.Log(value, 2)));
            }

            /// <summary>
            /// Calcula los coeficientes de la serie trigonometrica de Fourier
            /// para el elemento i dado, a partir de lo entregadopor la FFT
            /// </summary>
            /// <param name="complejos"></param>
            /// <param name="im"></param>
            /// <param name="re"></param>
            /// <param name="i">must be grater of 0</param>
            public static void FFTCoeficient(Complex32[] complejos, out double im,
                out double re, int i)
            {
                re = complejos[i].Real + complejos[complejos.Length - i].Real;

                im = complejos[complejos.Length - i].Imaginary - complejos[i].Imaginary;
            }

            /// <summary>
            /// Given a value, calculate the sampled value with a given window
            /// </summary>
            /// <param name="value"></param>
            /// <param name="i"></param>
            /// <param name="N"></param>
            /// <param name="SampleType"></param>
            /// <returns></returns>
            public static double Sample(double value, int i, int N, WindowType SampleType)
            {
                if (SampleType == WindowType.Rectangular)
                    return value;

                if (SampleType == WindowType.Triangular)
                {
                    //double wn = (N / 2 - Math.Abs(i - (N - 1) / 2));
                    //double wn = 1 - Math.Abs((2 * i - N + 1) / N);
                    double wn = 2.0 * i / (N - 2);
                    if (i > (N - 1) / 2)
                        wn = 2 - 2.0 * i / (N - 2);
                    double newvalue = value * wn;

                    return newvalue;
                }
                if (SampleType == WindowType.Hamming)
                {
                    double newvalue = value * (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (N - 1)));
                    return newvalue;
                }
                return value;
            }
        }

        public enum LineOptions { continuos, peeks }

        RectangleHighlight rectanglewindow;
        bool showwindow;
        public bool ShowWindow
        {
            get { return showwindow; }
            set {
                if (RaisePropertyChanged(value, ref showwindow))
                {
                    if (WindowPlotter == null)
                        return;
                    rectanglewindow.Height = 100;
                    rectanglewindow.Bounds = new DataRect(new System.Windows.Point(20, 20), rectanglewindow.RenderSize);
                    if (!WindowPlotter.Children.Contains(rectanglewindow))
                    {
                        WindowPlotter.Children.Add(rectanglewindow);
                        rectanglewindow.MouseDown += Rectanglewindow_MouseDown;
                    }
                }
            }
        }

        private void Rectanglewindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(WindowPlotter);
            rectanglewindow.Bounds= new DataRect(pos, rectanglewindow.RenderSize);
            //WindowPlotter.ser
        }

        //CircleElementPointMarker marker;

        //[Description("")]

        [Browsable(false)]
        public LineOptions ModuleOption { get; set; }
        [Browsable(false)]
        public LineOptions CosineOption { get; set; }
        [Browsable(false)]
        public LineOptions SineOption { get; set; }

        public TransientViewModel transientVM { get; set; }
        ObservableDataSource<Tuple<double, double>> DataInput = null;
        ObservableDataSource<Tuple<double, double>> DataOutput = null;
        ObservableDataSource<Tuple<double, double>> DataOutputReal = null;
        ObservableDataSource<Tuple<double, double>> DataOutputImag = null;

        //public ChartPlotter FFTPlotter { get; set; }

        LineGraph fftmodulegraph;
        public LineGraph FFTModuleGraph {
            get { return fftmodulegraph; }
            set { fftmodulegraph = value;
                fftmodulegraph.MouseDown += Fftlinegraph_MouseDown;
                }
        }

        private void Fftlinegraph_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectedObject = sender;
        }

        [Browsable(false)]
        public LineGraph FFTCosineGraph { get; set; }
        [Browsable(false)]
        public LineGraph FFTSineGraph { get; set; }

        //public ChartPlotter WindowPlotter { get; set; }
        [Browsable(false)]
        public LineGraph Windowlinegraph { get; set; }

        ManagedFourierTransformProvider fft;

        [Browsable(false)]
        public FFTWindow Window { get; set; }


        public FourierViewModel(/*TransientViewModel transientvm*/)
        {
            DataOutput = new ObservableDataSource<Tuple<double, double>>();
            DataOutputReal = new ObservableDataSource<Tuple<double, double>>();
            DataOutputImag = new ObservableDataSource<Tuple<double, double>>();
            DataInput = new ObservableDataSource<Tuple<double, double>>();
            Name = "Fourier Analisys";
            fft = new ManagedFourierTransformProvider();

            rectanglewindow = new RectangleHighlight();
            rectanglewindow.Width = 256;
            rectanglewindow.Stroke = Brushes.Red;
            rectanglewindow.StrokeThickness = 2;

           // marker = new CircleElementPointMarker();

            //transientVM = transientvm;
            Window = new FourierViewModel.FFTWindow();
            //Window.SampleType = FFTWindow.WindowType.Triangular;

            ModuleOption = LineOptions.peeks;
            SineOption = LineOptions.peeks;
            CosineOption = LineOptions.peeks;
            
            //ShowPlottCommand.CanExecuteTarget = (x) => 
            //                { return !string.IsNullOrWhiteSpace(TemporalExpression); };
            SimulateCommand.CanExecuteTarget = (x) =>
                            { return transientVM != null && transientVM.DataSource.Collection.Count > 0/*DataInput.Collection.Count > 0*/; };
        }

        

       


        protected override void OnShowTrack()
        {
            if (ShowTrack && !SpectrumPlotter.Children.Contains(mouseTrack))
                SpectrumPlotter.Children.Add(mouseTrack);
            else if (SpectrumPlotter.Children.Contains(mouseTrack))
                SpectrumPlotter.Children.Remove(mouseTrack);
        }

        public override void Simulate(object obj)
        {
            if (transientVM == null || transientVM.DataSource.Collection.Count == 0)
                return;

            ShowWindow = true;

            Legend.SetDescription(FFTModuleGraph, "Module");
            Legend.SetDescription(FFTCosineGraph, "Cosines");
            Legend.SetDescription(FFTSineGraph, "Sines");

            //SpectrumPlotter.Children.Add(marker);
            //marker.// FFTModuleGraph..Children.Add(marker);

            Window.NumberOfSamples = transientVM.DataSource.Collection.Count; // new ViewModel.FourierViewModel.FFTWindow() { };
            Window.NumberOfPoints = Window.NumberOfSamples;
            Complex32[] complejos = new Complex32[Window.NumberOfPoints];

            Window.TemporalFunction.Clear();
            Window.SampledFunction.Clear();
            for (int i = Window.StartAt; i < Window.FinishAt ; i++)
            {
                Window.TemporalFunction.Add(transientVM.DataSource.Collection[i].Item2);
                Window.SampledFunction.Add(
                            FFTWindow.Sample(transientVM.DataSource.Collection[i].Item2, i, 
                                            Window.NumberOfPoints, Window.SampleType));
                complejos[i] =new Complex32((float)Window.SampledFunction[i], 0);//result.Evaluate(i); 
            }

            //entrada
            DataInput.Collection.Clear();
            for (int i = 0; i < Window.SampledFunction.Count; i++)
            {
                Tuple<double, double> p = new Tuple<double, double>(i, Window.SampledFunction[i]);
                DataInput.Collection.Add(p);
            }
            DrawLine(DataInput, Windowlinegraph);



            fft.Forward(complejos, FourierTransformScaling.ForwardScaling);
            Window.FFTOutput.Clear();
            Window.FFTOutput.AddRange(complejos);

            //salida
            DataOutput.Collection.Clear();
            DataOutputReal.Collection.Clear();
            DataOutputImag.Collection.Clear();
            Tuple<double, double> im = new Tuple<double, double>(0, 0);
            Tuple<double, double> mo = new Tuple<double, double>(0, complejos[0].Magnitude);
            

            Tuple<double, double> re = new Tuple<double, double>(0, complejos[0].Real);
            DataOutputReal.Collection.Add(re);
            DataOutputImag.Collection.Add(im);
            DataOutput.Collection.Add(mo);
            for (int i = 1; i < Window.SampledFunction.Count / 2; i++)
            {
                if (SineOption == LineOptions.peeks)
                {
                    im = new Tuple<double, double>(i - 0.5, 0);
                    DataOutputImag.Collection.Add(im);
                }
                if (CosineOption == LineOptions.peeks)
                {
                    re = new Tuple<double, double>(i - 0.5, 0);
                    DataOutput.Collection.Add(mo);
                }
                if (ModuleOption == LineOptions.peeks)
                {
                    mo = new Tuple<double, double>(i - 0.5, 0);
                    DataOutputReal.Collection.Add(re);
                }
                double real, imag;
                FFTWindow.FFTCoeficient(complejos, out imag, out real, i);
                re = new Tuple<double, double>(i, real);
                im = new Tuple<double, double>(i, imag);
                mo = new Tuple<double, double>(i, (complejos[i] + complejos[Window.SampledFunction.Count - i].Conjugate()).Magnitude);
                //Tuple<double, double> p = new Tuple<double, double>(i, Math.Abs( numeros[i]));
                //Tuple<double, double> p = new Tuple<double, double>(i, numeros[i]);
                DataOutputReal.Collection.Add(re);
                DataOutputImag.Collection.Add(im);
                DataOutput.Collection.Add(mo);
            }
            DrawLine(DataOutput, FFTModuleGraph);
            DrawLine(DataOutputReal, FFTCosineGraph);
            DrawLine(DataOutputImag, FFTSineGraph);

        }


        #region storage plot methods

        public override void ShowPlot(object obj)
        {
           
        }

        public override void StoragePlot(object obj)
        {
            //if (!PlottedItems.Contains(linegraph))
            //    PlottedItems.Add(linegraph);
        }

       

        public override void DeletePlot(object obj)
        {
            //throw new NotImplementedException();
        }

        public override void Redraw(object obj)
        {
            //throw new NotImplementedException();
        }

        protected override void SaveAsFile(object obj)
        {
            //throw new NotImplementedException();
        }

        protected override void SaveFile(object obj)
        {
            //throw new NotImplementedException();
        }

        public override void Export(object obj)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
