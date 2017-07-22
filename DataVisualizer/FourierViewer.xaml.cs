using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Windows;

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para FourierViewer.xaml
    /// </summary>
    public partial class FourierViewer : Window
    {
        //FourierViewModel model;

        public FourierViewer()
        {
            InitializeComponent();

            Random rnd = new Random();
            var arr = new double[100];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = rnd.Next(100);
            }

            ObservableDataSource<Tuple<double, double>> DataInput = new ObservableDataSource<Tuple<double, double>>();

            //DataInput.Collection.Add()
            //plotter.DataSource =

            //four.FFTPlotter = plotter;
            //four.FFTlinegraph = linegraph;
        }

    }
}
