using Microsoft.Research.DynamicDataDisplay;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DataVisualizer.Converters
{
    public class TupleToLineGraphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Tuple<Plotter2D, LineGraph>)
                return ((Tuple<Plotter2D, LineGraph>)value).Item2;


            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
