using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CircuitMVVMBase.Converters
{
    public class ErrorToImageConverter :IValueConverter
    {
        static Dictionary<Notification.ErrorType, ImageSource> images;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (images == null)
            {
                images = new Dictionary<Notification.ErrorType, ImageSource>();
                //var resourceManager = new ResourceManager(typeof(Resources));
                //var bitmap = resourceManager.GetObject("Search") as System.Drawing.Bitmap;

                //var memoryStream = new MemoryStream();
                //bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                //memoryStream.Position = 0;

                //var bitmapImage = new BitmapImage();
                //bitmapImage.BeginInit();
                //bitmapImage.StreamSource = memoryStream;
                //bitmapImage.EndInit();

                images.Add(Notification.ErrorType.info,
                    new BitmapImage(new Uri("/Images/Errors/note.png", UriKind.Relative)));
                images.Add(Notification.ErrorType.warning,
                  new BitmapImage(new Uri("/Images/Errors/warning.png", UriKind.Relative)));
                images.Add(Notification.ErrorType.error,
                  new BitmapImage(new Uri("/Images/Errors/error.png", UriKind.Relative)));
                images.Add(Notification.ErrorType.exception,
                  new BitmapImage(new Uri("/Images/Errors/Note.png", UriKind.Relative)));
            }

            if (value is Notification.ErrorType)
            {
                Notification.ErrorType err = (Notification.ErrorType)value;
                return images[err];
            }
            return images[Notification.ErrorType.error];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        
    }
}
