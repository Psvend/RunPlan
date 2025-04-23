﻿using System;
using System.Globalization;
using Microsoft.Maui.Controls;


namespace RunPlan.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !(value is bool b) || !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => !(value is bool b) || !b;
    }
}
