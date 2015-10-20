using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Data.Models;
using System.Windows.Data;

namespace Client.Converters
{
    public class BookingTypeToStringConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetName(typeof(BookingType), value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(BookingType), (string)value);
        }
    }
}
