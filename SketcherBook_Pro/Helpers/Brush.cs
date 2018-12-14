using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace SketcherBook_Pro.Helpers {

	public sealed class Brush {

		public SolidColorBrush PreviewShapeStroke = new SolidColorBrush();

		public Color PreviewShapeStrokeColour { get; set; }

		public SolidColorBrush PreviewShapeFill = new SolidColorBrush();

		private readonly MainPage mainpage;

		public Brush(MainPage mainPage) {
			this.mainpage = mainPage;
			PreviewShapeStrokeColour = Colors.Gray;
			PreviewShapeStroke.Color = PreviewShapeStrokeColour;
		}

		public void BrushThicknessChanger() {
			mainpage.PenThickness = mainpage.BrushThinkessSlider.Value;

			if (mainpage.InkCanvas_Main != null) {
				var drawingAttributes = mainpage.InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
				var value = ((ComboBoxItem)mainpage.BrushStyleComboBox.SelectedItem).Content.ToString();

				if (value == ResourceLoader.GetForViewIndependentUse().GetString("BRUSH_STYLE_HIGHLIGHTER") || value == ResourceLoader.GetForViewIndependentUse().GetString("BRUSH_STYLE_CALLIGRAPHY")) {
					drawingAttributes.Size = new Size(
						mainpage.BrushThinkessSlider.Value,
						mainpage.BrushThinkessSlider.Value * 2);
				}
				else {
					drawingAttributes.Size = new Size(
						mainpage.BrushThinkessSlider.Value,
						mainpage.BrushThinkessSlider.Value);
				}
				mainpage.InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
			}

			DrawBrushSizePreview(mainpage.BrushThinkessSlider.Value, mainpage.BrushThinkessSlider.Value);
		}

		public void TriggerPenColourChange(object sender, SelectionChangedEventArgs e) {
			if (mainpage.InkCanvas_Main != null) {
				var drawingAttributes = mainpage.InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
				var colourListView = sender as ListView;

				if (colourListView?.SelectedItems.Count < 2) {
					var el = colourListView.SelectedItem as Rectangle;
					var bgbrush = el?.Fill as SolidColorBrush;
					if (bgbrush != null) {
						drawingAttributes.Color = bgbrush.Color;
					}
				}
				else {
					var el1 = colourListView?.SelectedItems[0] as Rectangle;
					var el2 = colourListView?.SelectedItems[1] as Rectangle;
					var brush1 = el1?.Fill as SolidColorBrush;
					var brush2 = el2?.Fill as SolidColorBrush;
					drawingAttributes.Color = MixColor(brush1, brush2);
				}
				mainpage.InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
			}
		}

		public void TriggerPenTypeChanged() {
			if (mainpage.InkCanvas_Main == null) {
				return;
			}

			if (mainpage.CustomPropertiesToggleSwitch == null) {
				return;
			}

			var drawingAttributes = mainpage.InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
			var value = ((ComboBoxItem)mainpage.BrushStyleComboBox.SelectedItem).Content.ToString();

			if (value == ResourceLoader.GetForViewIndependentUse().GetString("BRUSH_STYLE_BALLPOINT")) {
				drawingAttributes.Size = new Size(mainpage.PenThickness,
					mainpage.PenThickness);
				drawingAttributes.PenTip = PenTipShape.Circle;
				drawingAttributes.DrawAsHighlighter = false;
				drawingAttributes.PenTipTransform = Matrix3x2.Identity;
			}
			else if (value == ResourceLoader.GetForViewIndependentUse().GetString("BRUSH_STYLE_HIGHLIGHTER")) {
				drawingAttributes.Size = new Size(mainpage.PenThickness,
					mainpage.PenThickness * 2);
				drawingAttributes.PenTip = PenTipShape.Rectangle;
				drawingAttributes.DrawAsHighlighter = true;
				drawingAttributes.PenTipTransform = Matrix3x2.Identity;
			}
			else if (value == ResourceLoader.GetForViewIndependentUse().GetString("BRUSH_STYLE_CALLIGRAPHY")) {
				drawingAttributes.Size = new Size(mainpage.PenThickness,
					mainpage.PenThickness * 2);
				drawingAttributes.PenTip = PenTipShape.Rectangle;
				drawingAttributes.DrawAsHighlighter = false;
				const double calligraphyRadians = 45.0 * Math.PI / 180;
				drawingAttributes.PenTipTransform = Matrix3x2.CreateRotation((float)calligraphyRadians);
			}
			else if (value == ResourceLoader.GetForViewIndependentUse().GetString("BRUSH_STYLE_SQUARE")) {
				drawingAttributes.Size = new Size(mainpage.PenThickness,
					mainpage.PenThickness);
				drawingAttributes.PenTip = PenTipShape.Rectangle;
				drawingAttributes.DrawAsHighlighter = false;
				const double calligraphyRadians = 45.0 * Math.PI / 180;
				drawingAttributes.PenTipTransform = Matrix3x2.CreateRotation((float)calligraphyRadians);
			}

			mainpage.InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
		}

		private static Color MixColor(SolidColorBrush b1, SolidColorBrush b2) {
			return new SolidColorBrush(
				Color.FromArgb(
					Math.Max(b1.Color.A, b2.Color.A),
					(byte)((b1.Color.R + b2.Color.R) / 2),
					(byte)((b1.Color.G + b2.Color.G) / 2),
					(byte)((b1.Color.B + b2.Color.B) / 2))).Color;
		}

		public async void DrawBrushSizePreview(double width, double height) {
			if (mainpage.InkGrid == null) {
				return;
			}

			if (mainpage.CustomPropertiesHighlighterToggleSwitch == null) {
				return;
			}

			PreviewShapeFill.Color = mainpage.CustomPropertiesHighlighterToggleSwitch.IsOn ? Color.FromArgb(100, 255, 255, 0) : Colors.Transparent;
			var drawingAttributes = mainpage.InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
			var pos = Window.Current.CoreWindow.PointerPosition;

			Shape previewShape = null;

			if (drawingAttributes.PenTip == PenTipShape.Circle) {
				previewShape = new Ellipse
				{
					Fill = PreviewShapeFill,
					Stroke = PreviewShapeStroke,
					StrokeThickness = 3,
					Width = width,
					Height = height,
					Tag = "BrushSizePreview"
				};

			}
			else {
				previewShape = new Rectangle
				{
					Fill = PreviewShapeFill,
					Stroke = PreviewShapeStroke,
					StrokeThickness = 3,
					Width = width,
					Height = height,
					Tag = "BrushSizePreview"
				};
			}

			for (var i = 1; i < mainpage.InkGrid.Children.Count; i++) {
				mainpage.InkGrid.Children.RemoveAt(i);
			}

			mainpage.InkGrid.Children.Add(previewShape);
			await Task.Delay(TimeSpan.FromSeconds(1));

			var opacityAnim = new DoubleAnimation
			{
				Duration = new Duration(TimeSpan.FromSeconds(0.2)),
				To = 0,
				EasingFunction = new ExponentialEase()
			};

			var storyBoard = new Storyboard
			{
				Duration = new Duration(TimeSpan.FromSeconds(0.2))
			};

			storyBoard.Children.Add(opacityAnim);
			Storyboard.SetTarget(opacityAnim, previewShape);
			Storyboard.SetTargetProperty(opacityAnim, "Opacity");
			storyBoard.Begin();

			await Task.Delay(TimeSpan.FromSeconds(0.2));
			mainpage.InkGrid.Children.Remove(previewShape);
		}
	}
}
