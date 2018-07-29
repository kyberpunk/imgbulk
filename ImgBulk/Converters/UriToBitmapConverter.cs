using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ImgBulk.Converters
{
    public class UriToBitmapConverter : IValueConverter
   {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.DecodePixelWidth = 100;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(value.ToString());
                bi.EndInit();
                return bi;
            }
            catch (NotSupportedException ex)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri("Images/imgerror.png", UriKind.Relative);
                bi.EndInit();
                return bi;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
  }
}
