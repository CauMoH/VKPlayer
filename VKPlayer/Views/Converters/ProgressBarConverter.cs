using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKPlayer.Views.Converters
{
    public class ProgressBarConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                var value = values[0];
                var maximum = values[1];
                if (value == null || value == DependencyProperty.UnsetValue ||
                    maximum == null || value == DependencyProperty.UnsetValue)
                {
                    return false;
                }

                if (System.Convert.ToUInt32(value) == System.Convert.ToUInt32(maximum))
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
