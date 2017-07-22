using ElectricalAnalysis;
using MathNet.Numerics;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using static DataVisualizer.MVVM.ViewModel.FourierViewModel;

namespace DataVisualizer.MVVM.ViewModel
{

    public class InverseFourierViewModel : TransformViewModelBase
    {

        //[Browsable(false)]
        ObservableDataSource<Tuple<double, double>> DataInput = null;
        ObservableDataSource<Tuple<double, double>> DataOutput = null;
        

        //List<Complex32> spectrum;
        [Description("List of complex values is the SPECTRUM")]
        public List<Complex32> Spectrum {
            get;// { return spectrum; }
            protected set;// { }
        }

        [Browsable(false)]
        public FourierViewModel FFTViewModel { get; set; }

        //ManagedFourierTransformProvider fft;

        int harmoniccount;
        /// <summary>
        /// Maximus number of harmonics showed in temporal plot
        /// </summary>
        [Description("Maximus number of harmonics showed in temporal plot")]
        public int HarmonicCount
        {
            get { return harmoniccount; }
            set {
                if (RaisePropertyChanged(value, ref harmoniccount))
                    createHarmonics();
                }
        }

        /// <summary>
        /// Simplified list of Harmonics plots
        /// </summary>
        [Browsable(false)]
        public ObservableCollection<LineGraph> Harmonics { get; protected set; }


        public InverseFourierViewModel()
        {
            Name = "IFFT Analisys";
            DataInput = new ObservableDataSource<Tuple<double, double>>();
            DataOutput = new ObservableDataSource<Tuple<double, double>>();
            Spectrum = new List<Complex32>();
            Harmonics = new ObservableCollection<LineGraph>();
            HarmonicCount = 10;
            //fft = new ManagedFourierTransformProvider();

            SimulateCommand.CanExecuteTarget = (x) =>
            { return FFTViewModel != null && FFTViewModel.Window.FFTOutput != null; };
        }

        protected void createHarmonics()
        {
            if (WindowPlotter != null)
            {
                foreach (var item in Harmonics)
                {
                    WindowPlotter.Children.Remove(item);
                }
            }

            Harmonics.Clear();
            LineGraph line;
            for (int i = 0; i <= HarmonicCount + 1; i++)
            {
                line = new LineGraph();
                if (i == 0)
                {
                    line.Name = "DC_component";
                    //line.ToolTip = "DC component";
                    line.Stroke = Brushes.Red;
                    line.StrokeThickness = 1;
                }
                else if (i == HarmonicCount)
                {
                    line.Name = "Other_Components";
                    //line.ToolTip = "Remanent Components";
                    line.Stroke = Brushes.Green;
                }
                else if (i == HarmonicCount + 1)
                {
                    line.Name = "Harmonics_Sum";
                    //line.ToolTip = "Harmonics Sum";
                    line.Stroke = Brushes.Brown;
                    line.StrokeThickness = 2;
                }
                else
                {
                    line.Name = "Component_" + i.ToString();
                    //line.ToolTip = "Component " + i.ToString();
                    byte alfa = (byte)(255 - i * 20);
                    if (i > 10)
                        alfa = 50;
                    line.Stroke = new SolidColorBrush(Color.FromArgb(alfa, 155, 155, 155));//Brushes.Gray;
                    line.StrokeThickness = 2;
                }
                Harmonics.Add(line);
            }
            //if (!WindowPlotter.Children.Contains(line0))
            if (WindowPlotter != null)
                WindowPlotter.Children.AddMany(Harmonics);

        }

        protected override void OnShowTrack()
        {
            if (ShowTrack && !WindowPlotter.Children.Contains(mouseTrack))
                WindowPlotter.Children.Add(mouseTrack);
            else if (WindowPlotter.Children.Contains(mouseTrack))
                WindowPlotter.Children.Remove(mouseTrack);
        }

        public override void Simulate(object obj)
        {
            if (FFTViewModel.FFTModuleGraph.DataSource == null)
                return;

            DataInput = FFTViewModel.FFTModuleGraph.DataSource as ObservableDataSource<Tuple<double, double>>;
            Spectrum = FFTViewModel.Window.FFTOutput;
            if (Spectrum.Count < 0)
                return;

            createHarmonics();

            //for (int i = 0; i < DataInput.Collection.Count / 2; i++)
            //    Spectrum.Add(DataInput.Collection[i].Item2);
            int N = FFTViewModel.Window.FFTOutput.Count;

            List<double> othervalues = new List<double>(new double[N]);
            List<double> sumvalues = new List<double>(new double[N]);
            ObservableDataSource<Tuple<double, double>> datasum = new ObservableDataSource<Tuple<double, double>>();
            ObservableDataSource<Tuple<double, double>> dataothers = new ObservableDataSource<Tuple<double, double>>();
            ObservableDataSource<Tuple<double, double>> data;
            for (int i = 1; i < N / 2 ; i++)
            {
                //armonico i
                double real, imag;
                FFTWindow.FFTCoeficient(Spectrum.ToArray(), out imag, out real, i);

                //double module = Complex32.Abs(new Complex32((float)real, (float)imag));
                Complex32 valor = new Complex32((float)real, (float)imag);
                //double phase = valor.Phase;

                data = new ObservableDataSource<Tuple<double, double>>();

                for (int j = 0; j < N; j++)
                {
                    //time j
                    double sin = sine(valor.Magnitude, valor.Phase, j, i, Spectrum.Count);
                    sumvalues[j] += sin;
                    if (i < Harmonics.Count)
                        data.Collection.Add(new Tuple<double, double>(j, sin));
                    else //if (i >)
                        othervalues[j] += sin;
                }
                if (i < HarmonicCount)
                {
                    LineGraph line = Harmonics[i];
                    line.ToolTip = StringUtils.CodeString(valor.Magnitude) + 
                        "*Sin(" + i.ToString() + "ω\x2081 *t+" 
                        + StringUtils.CodeString(valor.Phase) + ")";
                    DrawLine(data, line);
                    if (i == 1)
                        Legend.SetDescription(line, "Fundamental");
                    else
                        Legend.SetDescription(line, "Component " + i.ToString());
                }
            }

            for (int j = 0; j < N; j++)
            {
                //time j
                datasum.Collection.Add(new Tuple<double, double>(j, sumvalues[j]));
                dataothers.Collection.Add(new Tuple<double, double>(j, othervalues[j]));
            }

            data = new ObservableDataSource<Tuple<double, double>>();
            data.Collection.Add(new Tuple<double, double>(0, Spectrum[0].Real));
            data.Collection.Add(new Tuple<double, double>(Spectrum.Count -1, Spectrum[0].Real));
            LineGraph line0 = Harmonics[0];
            line0.ToolTip = StringUtils.CodeString(Spectrum[0].Real);
            DrawLine(data, line0);
            Legend.SetDescription(line0, "DC Component");

            LineGraph lineall = Harmonics[Harmonics.Count- 1];
            DrawLine(datasum, lineall);
            lineall.ToolTip = "Sum of main harmonics";
            Legend.SetDescription(lineall, lineall.ToolTip);

            LineGraph lineL = Harmonics[Harmonics.Count - 2];
            DrawLine(dataothers, lineL);
            lineL.ToolTip = "remain harmonics";
            Legend.SetDescription(lineL, lineL.ToolTip);

            //if (!WindowPlotter.Children.Contains(line0))
            //    WindowPlotter.Children.AddMany(Harmonics);

        }



        /// <summary>
        /// Calculate sin with the given parameters
        /// </summary>
        /// <param name="module"></param>
        /// <param name="phase"></param>
        /// <param name="t">time</param>
        /// <param name="n">harmonic number</param>
        /// <param name="lenght">number of samples</param>
        /// <returns></returns>
        public static double sine(double module, double phase, int t, int n,int lenght)
        {
            return module * Math.Sin(2 * Math.PI * n * t / (lenght - 1) + Math.PI / 2 - phase);
        }

        public override void DeletePlot(object obj)
        {
        }

        public override void ShowPlot(object obj)
        {
        }


        public override void StoragePlot(object obj)
        {
        }

        public override void Redraw(object obj)
        {
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
    }
}
