using CircuitMVVMBase.MVVM.ViewModel;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataVisualizer.MVVM.ViewModel
{
    public abstract class TransformViewModelBase: Plotter2DViewModel
    {

        [Browsable(false)]
        public ChartPlotter SpectrumPlotter { get; set; }
        [Browsable(false)]
        public ChartPlotter WindowPlotter { get; set; }

        protected void DrawLine(ObservableDataSource<Tuple<double, double>> data, LineGraph linegraph)
        {
            
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
        }

    }
}
