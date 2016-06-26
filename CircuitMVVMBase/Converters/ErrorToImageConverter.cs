using CircuitMVVMBase.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
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

                BitmapImage bitmapImage = FindResource("note");
                images.Add(Notification.ErrorType.info, bitmapImage);
                //images.Add(Notification.ErrorType.info,
                //    new BitmapImage(new Uri("Images/Errors/note.png", UriKind.Relative)));
                images.Add(Notification.ErrorType.warning, FindResource("warning"));
                //new BitmapImage(new Uri("/Images/Errors/warning.png", UriKind.Relative)));
                images.Add(Notification.ErrorType.error, FindResource("error"));
                //new BitmapImage(new Uri("/Images/Errors/error.png", UriKind.Relative)));
                images.Add(Notification.ErrorType.exception, FindResource("bug_red"));
                  //new BitmapImage(new Uri("/Images/Errors/Note.png", UriKind.Relative)));
            }

            if (value is Notification.ErrorType)
            {
                Notification.ErrorType err = (Notification.ErrorType)value;
                return images[err];
            }
            return images[Notification.ErrorType.error];
        }

        private static BitmapImage FindResource(string name)
        {
            var resourceManager = new ResourceManager(typeof(Resources));
            var bitmap = resourceManager.GetObject(name) as System.Drawing.Bitmap;

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        
    }
}
