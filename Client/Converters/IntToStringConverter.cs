using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Client.Converters
{
    class IntToStringConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                return null;
            return System.Convert.ToString((int)value).PadLeft(2, '0');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(int))
                return null;
            return System.Convert.ToInt32((string)value);
        }
    }
}
