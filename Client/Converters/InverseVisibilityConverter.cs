using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Client.Converters
{
    class InverseVisibilityConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility) && targetType != typeof(Visibility))
                throw new ArgumentException("Target must be a Visibility.");

            return ((Visibility)value) == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}