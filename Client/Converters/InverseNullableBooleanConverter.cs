﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    class InverseNullableBooleanConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool?) && targetType != typeof(bool))
                throw new ArgumentException("Target must be a boolean.");

            return !(bool?)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
