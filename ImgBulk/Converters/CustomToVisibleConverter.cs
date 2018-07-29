using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImgBulk.Converters
{
    class CustomToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string && ((string) value) == "Custom" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
