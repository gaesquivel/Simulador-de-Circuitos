using ElectricalAnalysis;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MathNet.Numerics.Providers.FourierTransform;
using MathParser;
using CircuitMVVMBase.MVVM.ViewModel;

namespace DataVisualizer.MVVM.ViewModel
{
    public class FourierViewModel : Plotter2DViewModel
    {

        Parser parse;
        ParsingResult result;

        bool ischanged;
        public bool IsChanged
        {
            get { return ischanged; }
        }

        ObservableDataSource<Tuple<double, double>> DataInput = null;
        ObservableDataSource<Tuple<double, double>> DataOutput = null;
        public ChartPlotter Plotter { get; set; }
        public LineGraph linegraph { get; set; }

        ManagedFourierTransformProvider fft;

        string expression;
        public string TemporalExpression
        {
            get { return expression; }
            set { ischanged = RaisePropertyChanged(value, ref expression); }
        }


        public FourierViewModel()
        {
            DataOutput = new ObservableDataSource<Tuple<double, double>>();
            DataInput = new ObservableDataSource<Tuple<double, double>>();
            Name = "Fourier Analisys";
            fft = new ManagedFourierTransformProvider();
            parse = new Parser();
            parse.Parameters.Add("x");

            Initialize();
            TemporalExpression = "10*sin(2*pi*x/64)";
            ShowPlottCommand.CanExecuteTarget = (x) => 
                            { return !string.IsNullOrWhiteSpace(TemporalExpression); };
            SimulateCommand.CanExecuteTarget = (x) =>
                            { return DataInput != null && DataInput.Collection.Count > 0; };
        }

        private void Initialize()
        {
            // Create first source
            //DataSource = new ObservableDataSource<Tuple<double, double>>();
            if (Plotter == null)
                return;

            DataOutput.SetXYMapping(z => {
                Point p = new Point(z.Item1, z.Item2);
                return p;
            });
            HorizontalAxis axis = (HorizontalAxis)Plotter.MainHorizontalAxis;
            axis.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));

            VerticalAxis axis2 = (VerticalAxis)Plotter.MainVerticalAxis;
            axis2.LabelProvider.SetCustomFormatter(info => StringUtils.CodeString(info.Tick));



            linegraph.DataSource = DataOutput;
        }

       

        public override void ShowPlot(object obj)
        {
            //result = parse.Parse("10*sin(x)");
            try
            {
                result = parse.Parse(TemporalExpression);
            }
            catch (Exception ex)
            {
                return;
            }

            //Grammar gr = new Grammar();
            //foreach (var item in gr.RegisteredFunctions)
            //{
            //    Console.WriteLine(item.Name);
            //}
            //result.Evaluate()
            double[] numeros = new double[64];

            for (int i = 0; i < numeros.Length; i++)
            {
                numeros[i] = result.Evaluate(i);
            }
            DataInput.Collection.Clear();

            for (int i = 0; i < numeros.Length; i++)
            {
                Tuple<double, double> p = new Tuple<double, double>(i, numeros[i]);
                DataInput.Collection.Add(p);
            }
            StoragePlot(DataInput);

            ischanged = false;
        }

        public override void Simulate(object obj)
        {
            if (result == null)
                return;

            double[] numeros = new double[64];

            for (int i = 0; i < numeros.Length ; i++)
            {
                numeros[i] = result.Evaluate(i); 
            }
            fft.ForwardReal(numeros, numeros.Length - 1, FourierTransformScaling.ForwardScaling);
            DataOutput.Collection.Clear();

            for (int i = 0; i < numeros.Length; i++)
            {
                Tuple<double, double> p = new Tuple<double, double>(i, Math.Abs( numeros[i]));
                DataOutput.Collection.Add(p);
            }
            StoragePlot(DataOutput);
            
        }

        public override void StoragePlot(object obj)
        {
            var data = obj as ObservableDataSource<Tuple<double, double>>;
            if (data == null)
                return;

            if (linegraph != null)
            {
                //linegraph = new LineGraph();
                linegraph.Name = "";
                //line.
                int n = data.Collection.Count;
                Tuple<double, double>[] arr = new Tuple<double, double>[n];
                data.Collection.CopyTo(arr, 0);
                var col = new ObservableDataSource<Tuple<double, double>>(arr);
                col.SetXYMapping(z =>
                {
                    Point p = new Point(z.Item1, z.Item2);
                    return p;
                });

                linegraph.DataSource = col;
                PlottedItems.Add(linegraph);
                
            }
        }

        protected override void Redraw(object obj)
        {
            throw new NotImplementedException();
        }

        public override void DeletePlot(object obj)
        {
            throw new NotImplementedException();
        }

        protected override void SaveAsFile(object obj)
        {
            throw new NotImplementedException();
        }

        protected override void SaveFile(object obj)
        {
            throw new NotImplementedException();
        }

        public override void Export(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
