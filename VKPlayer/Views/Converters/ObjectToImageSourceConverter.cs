using System;
using System.Windows.Data;
using System.Windows.Media;

namespace VKPlayer.Views.Converters
{
    public class ObjectToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return new object();
            }
            var packUri = value.ToString();
            var imageObject = new ImageSourceConverter().ConvertFromString(packUri);
            if (imageObject != null)
            {
                return (ImageSource) imageObject;
            }
            else
            {
                return new object();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return new object();
        }
    }
}
