using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TahoTibor
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            bool invert = parameter != null && parameter.ToString().ToLower() == "inverse";

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            bool invert = parameter != null && parameter.ToString().ToLower() == "inverse";
            bool result = visibility == Visibility.Visible;

            return invert ? !result : result;
        }
    }
}