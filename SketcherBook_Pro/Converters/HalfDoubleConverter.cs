using System;
using Windows.UI.Xaml.Data;


namespace SketcherBook_Pro.Converters
{
    class HalfDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double t = (double)value;
            return t / 2;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double t = (double)value;
            return t * 2;
        }
    }
}
