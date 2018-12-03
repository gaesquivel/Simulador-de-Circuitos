using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CircuitMVVMBase.Converters
{
    public class ErrorToColorConverter :IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Notification.ErrorType)
            {
                Notification.ErrorType err = (Notification.ErrorType)value ;
                switch (err)
                {
                    case Notification.ErrorType.info:
                        return new SolidColorBrush(Colors.LightGoldenrodYellow);
                    case Notification.ErrorType.warning:
                        return new SolidColorBrush(Colors.Orange);
                    case Notification.ErrorType.error:
                        return new SolidColorBrush(Colors.OrangeRed);
                    default:
                        return new SolidColorBrush(Colors.LightGray);
                }

            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        
    }
}
