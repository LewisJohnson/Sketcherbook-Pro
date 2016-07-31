using System;
using Windows.UI.Xaml.Data;

namespace SketcherBook_Pro.Converters
{

    public class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return double.Parse(value.ToString());
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return int.Parse(value.ToString());
        }
    }

}
