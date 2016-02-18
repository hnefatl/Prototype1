using System;
using System.Linq;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    class IntToStringConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
                return System.Convert.ToString((int)value);
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                if (!string.IsNullOrWhiteSpace(value as string))
                    return System.Convert.ToInt32(new string((value as string).Where(c => char.IsDigit(c)).ToArray()).PadLeft(1, '0'));
            return 0;
        }
    }
}
