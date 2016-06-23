using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElectricalAnalysis
{
    public class ImageUtils
    {

        public static BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           System.Windows.Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return i;
        }

        [Obsolete]
        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                return bi;
            }
        }

        public static System.Windows.Media.Brush CreateComplexBrush(Bitmap bmp)
        {

            System.Windows.Media.Brush b = new ImageBrush(Bitmap2BitmapSource(bmp));
            return b;
        }

        private const double TwoPI = 2.0 * Math.PI;

        // Claudio Rocchini used this color mapping
        // in his image at http://en.wikipedia.org/wiki/File:Color_complex_plot.jpg
        // for the Wikipedia article on "Complex Analysis"
        public static Tuple<double, double, double> ComplexToHsv(Complex z)
        {

            // extract a phase 0 <= t < 2 pi
            double t = z.Phase;
            while (t < 0.0) t += TwoPI;
            while (t >= TwoPI) t -= TwoPI;

            // the hue is determined by the phase
            double h = t / TwoPI;

            // extract a magnitude m >= 0
            double m = z.Magnitude;

            // map the magnitude logrithmicly into the repeating interval 0 < r < 1
            // this is essentially where we are between countour lines
            double r0 = 0.0;
            double r1 = 1.0;
            while (m > r1)
            {
                r0 = r1;
                r1 = r1 * Math.E;
            }
            double r = (m - r0) / (r1 - r0);
            // this puts contour lines at 0, 1, e, e^2, e^3, ...

            // determine saturation and value based on r
            // p and q are complementary distances from a countour line
            double p = r < 0.5 ? 2.0 * r : 2.0 * (1.0 - r);
            double q = 1.0 - p;
            // only let p and q go to zero very close to zero; otherwise they should stay nearly 1
            // this keep the countour lines from getting thick
            double p1 = 1 - q * q * q;
            double q1 = 1 - p * p * p;
            // fix s and v from p1 and q1
            double s = 0.4 + 0.6 * p1;
            double v = 0.6 + 0.4 * q1;

            return new Tuple<double, double, double>( h, s, v );

        }

        // convert HSV to RGB

        public static Tuple<double, double, double> HsvToRgb(Tuple<double, double, double> hsv)
        {

            double h = hsv.Item1;
            double s = hsv.Item2;
            double v = hsv.Item3;

            double r, g, b;
            if (s == 0)
            {
                r = g = b = v;
            }
            else
            {
                if (h == 1.0) h = 0.0;
                double z = Math.Truncate(6 * h); int i = (int)z;
                double f = h * 6 - z;
                double p = v * (1 - s);
                double q = v * (1 - s * f);
                double t = v * (1 - s * (1 - f));

                switch (i)
                {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                    default: throw new InvalidOperationException();
                }
            }

            return new Tuple<double, double, double>( r, g,  b );

        }

        
    
    }
}
