using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SketcherBook_Pro.Helpers
{

	public sealed class CanvasBrush
	{
		private InkDrawingAttributes inkDrawingAttributes;

		public CanvasBrush()
		{
			inkDrawingAttributes = new InkDrawingAttributes();
		}

		public InkDrawingAttributes UpdateBrushSize(BrushStyle brushStyle, double width, double height)
		{
			if (brushStyle == BrushStyle.HIGHLIGHTER || brushStyle == BrushStyle.CALLIGRAPHY)
			{
				height *= 2;
			}

			inkDrawingAttributes.Size = new Size(width, height);

			return inkDrawingAttributes;
		}

		internal void AdjustBrushSize(double value)
		{
			double newSize = inkDrawingAttributes.Size.Width + value;
			UpdateBrushSize(BrushStyle.SQUARE, newSize, newSize);
		}

		public InkDrawingAttributes UpdateBrushColour(Color colour, Color? mixColour = null)
		{
			if (mixColour == null)
			{
				inkDrawingAttributes.Color = colour;
			}
			else
			{
				inkDrawingAttributes.Color = MixColor(colour, mixColour.Value);
			}

			return inkDrawingAttributes;
		}

		public InkDrawingAttributes UpdateBrushType(BrushStyle brushStyle)
		{
			switch (brushStyle)
			{
				case BrushStyle.HIGHLIGHTER:
					inkDrawingAttributes.PenTip = PenTipShape.Rectangle;
					inkDrawingAttributes.PenTipTransform = Matrix3x2.Identity;
					inkDrawingAttributes.DrawAsHighlighter = true;
					break;

				case BrushStyle.BALLPOINT:
					inkDrawingAttributes.PenTip = PenTipShape.Circle;
					inkDrawingAttributes.PenTipTransform = Matrix3x2.Identity;
					inkDrawingAttributes.DrawAsHighlighter = false;
					break;

				case BrushStyle.CALLIGRAPHY:
					double calligraphyRadians = 45.0 * Math.PI / 180;

					inkDrawingAttributes.PenTip = PenTipShape.Rectangle;
					inkDrawingAttributes.PenTipTransform = Matrix3x2.CreateRotation((float)calligraphyRadians);
					inkDrawingAttributes.DrawAsHighlighter = false;
					break;

				case BrushStyle.SQUARE:
					inkDrawingAttributes.PenTip = PenTipShape.Rectangle;
					inkDrawingAttributes.DrawAsHighlighter = false;
					break;
			}

			return inkDrawingAttributes;
		}

		private static Color MixColor(Color colour, Color mixColour)
		{
			return new SolidColorBrush(
				Color.FromArgb(
					Math.Max(colour.A, mixColour.A),
					(byte)((colour.R + mixColour.R) / 2),
					(byte)((colour.G + mixColour.G) / 2),
					(byte)((colour.B + mixColour.B) / 2))).Color;
		}

		public async void DrawBrushSizePreview(double width, double height)
		{
			//var preivreColour = mainpage.CustomPropertiesHighlighterToggleSwitch.IsOn ? Color.FromArgb(100, 255, 255, 0) : Colors.Transparent;
			// previewShapeStroke.Color = Colors.Gray;
			//InkDrawingAttributes drawingAttributes = mainpage.InkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
			//Point pos = Window.Current.CoreWindow.PointerPosition;

			//Shape previewShape = null;

			//if (drawingAttributes.PenTip == PenTipShape.Circle)
			//{
			//	previewShape = new Ellipse
			//	{
			//		Fill = PreviewShapeFill,
			//		Stroke = PreviewShapeStroke,
			//		StrokeThickness = 3,
			//		Width = width,
			//		Height = height,
			//		Tag = "BrushSizePreview"
			//	};

			//}
			//else
			//{
			//	previewShape = new Rectangle
			//	{
			//		Fill = PreviewShapeFill,
			//		Stroke = PreviewShapeStroke,
			//		StrokeThickness = 3,
			//		Width = width,
			//		Height = height,
			//		Tag = "BrushSizePreview"
			//	};
			//}

			//for (int i = 1; i < mainpage.InkGrid.Children.Count; i++)
			//{
			//	mainpage.InkGrid.Children.RemoveAt(i);
			//}

			//mainpage.InkGrid.Children.Add(previewShape);
			//await Task.Delay(TimeSpan.FromSeconds(1));

			//DoubleAnimation opacityAnim = new DoubleAnimation
			//{
			//	Duration = new Duration(TimeSpan.FromSeconds(0.2)),
			//	To = 0,
			//	EasingFunction = new ExponentialEase()
			//};

			//Storyboard storyBoard = new Storyboard
			//{
			//	Duration = new Duration(TimeSpan.FromSeconds(0.2))
			//};

			//storyBoard.Children.Add(opacityAnim);
			//Storyboard.SetTarget(opacityAnim, previewShape);
			//Storyboard.SetTargetProperty(opacityAnim, "Opacity");
			//storyBoard.Begin();

			//await Task.Delay(TimeSpan.FromSeconds(0.2));
			//mainpage.InkGrid.Children.Remove(previewShape);
		}
	}
}
