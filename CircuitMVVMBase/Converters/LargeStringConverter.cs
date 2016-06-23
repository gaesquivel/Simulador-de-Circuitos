using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace CircuitMVVMBase.Converters
{
    public class LargeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = "";
            if (value is string)
            {
                val = value as string;
                if (val.Contains("\\"))
                    return val = "..." + val.Substring(val.LastIndexOf("\\"));

                return val = "..." + Path.GetFileName(value as string);
            }
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
