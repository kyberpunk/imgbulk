using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImgBulk.Converters
{
    class ImageFormatToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(ImageFormat.Jpeg) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
