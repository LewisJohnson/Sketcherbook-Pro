using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;

namespace SketcherBook_Pro.Helpers
{
    public sealed class Initialisers
    {
        public static ObservableCollection<SolidColorBrush> PopulateInitialColourCollection()
        {
            return new ObservableCollection<SolidColorBrush>
            {
                new SolidColorBrush(Colors.White),
                new SolidColorBrush(Colors.LightGray),
                new SolidColorBrush(Colors.Gray),
                new SolidColorBrush(Colors.Black),
                new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.OrangeRed),
                new SolidColorBrush(Colors.Orange),
                new SolidColorBrush(Colors.Yellow),
                new SolidColorBrush(Colors.YellowGreen),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.LightBlue),
                new SolidColorBrush(Colors.Blue),
                new SolidColorBrush(Colors.LavenderBlush),
                new SolidColorBrush(Colors.Violet),
                new SolidColorBrush(Colors.BlueViolet),
                new SolidColorBrush(Colors.Indigo)
            };
        }

        public static InkDrawingAttributes InitialDrawingAttributes()
        {
            return new InkDrawingAttributes
            {
                Color = Colors.Black,
                Size = new Size(5, 5),
                IgnorePressure = false,
                FitToCurve = true
            };
        }
    }
}