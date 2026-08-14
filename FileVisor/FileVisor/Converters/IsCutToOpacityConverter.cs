using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileVisor.Converters
{
    internal class IsCutToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return boolean ? (double) Application.Current.Resources["DisabledElementOpacity"] : 1.0;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
