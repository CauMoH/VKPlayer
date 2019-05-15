using System;
using System.Windows.Data;
using System.Windows.Media;

namespace VKPlayer.Views.Converters
{
    public class ObjectToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var packUri = value.ToString();
            var source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
