using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class BooleanToVisibilityConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new ArgumentException("value was not of type bool.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return ((Visibility)value) == Visibility.Visible;
            }
            throw new ArgumentException("value was not of type Visibility.");
        }
    }
}
