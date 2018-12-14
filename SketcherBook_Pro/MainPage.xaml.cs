using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using SketcherBook_Pro.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using CanvasBrush = SketcherBook_Pro.Helpers.CanvasBrush;

namespace SketcherBook_Pro
{

	public sealed partial class MainPage
	{
		public ImageProcessing ImageProcessing { get; set; }
		public CanvasBrush Brush { get; set; }
		public static KeyboardShortcuts KeyboardShortcuts { get; private set; }

		private readonly ObservableCollection<InkAction> cachedInkStrokesCollection = new ObservableCollection<InkAction>();
		private readonly ObservableCollection<Rectangle> brushColourCollection = new ObservableCollection<Rectangle>();
		private readonly ICollection<Rectangle> dragedColoursCollection = new List<Rectangle>();
		private readonly ICollection<InkLayer> dragedLayersCollection = new List<InkLayer>();

		private const char ExpandToFullScreen = '';
		private const char ShrinkFromFullScreen = '';
		private const char ExpandPanelCharcter = '<';
		private const char ShrinkPanelCharcter = '>';
		private const char LayersLockedCharcter = '';
		private const char LayersUnLockedCharcter = '';

		public MainPage()
		{
			ImageProcessing = new ImageProcessing();
			KeyboardShortcuts = new KeyboardShortcuts();
			Brush = new CanvasBrush();

			InitializeComponent();

			ObservableCollection<SolidColorBrush> defaultBrushColourCollection = Initialisers.PopulateInitialColourCollection();

			foreach (SolidColorBrush colourOption in defaultBrushColourCollection)
			{
				brushColourCollection.Add(new Rectangle
				{
					Style = (Style)Application.Current.Resources["ColourOptionRectangle"],
					Fill = colourOption
				});
			}

			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(Initialisers.InitialDrawingAttributes());
			InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;

			PopulateElements();
			SetTriggers();
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			IntroAnimation.Begin();
		}

		private void PopulateElements()
		{
			InkCanvas.InkPresenter.StrokeContainer = LayerManager.GlobalInkLayers.First().LayerStrokeContainer;
			ColourListView.ItemsSource = brushColourCollection;
			LayersListBox.ItemsSource = LayerManager.GlobalInkLayers;

			IEnumerable<BrushStyle> brushStyles = Enum.GetValues(typeof(BrushStyle)).Cast<BrushStyle>();
			BrushStyleComboBox.ItemsSource = brushStyles.ToList();
			BrushStyleComboBox.SelectedIndex = 0;
		}

		private void SetTriggers()
		{
			InkCanvas.InkPresenter.StrokesCollected += InkPresenterOnStrokesCollected;
			InkCanvas.InkPresenter.StrokesErased += InkPresenterOnStrokesErased;
		}

		//Undo Functions
		private void InkPresenterOnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
		{
			foreach (InkStroke item in args.Strokes)
			{
				cachedInkStrokesCollection.Add(new InkAction(item, InkActionType.Erased));
			}
		}
		private void InkPresenterOnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
		{
			foreach (InkStroke item in args.Strokes)
			{
				cachedInkStrokesCollection.Add(new InkAction(item, InkActionType.Collected));
			}

		}
		private void UndoButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			InkAction item = cachedInkStrokesCollection.Last();
			if (item.Action == InkActionType.Collected)
			{
				cachedInkStrokesCollection.Remove(item);
			}
			else if (item.Action == InkActionType.Erased)
			{
				cachedInkStrokesCollection.Remove(item);
			}
		}

		private void OnClear(object sender, RoutedEventArgs e)
		{
			InkCanvas.InkPresenter.StrokeContainer.Clear();
		}

		private void OnSaveAsync(object sender, RoutedEventArgs e)
		{
			ImageProcessing.OnSaveAsync(InkCanvas.InkPresenter.StrokeContainer);
		}

		private void OnLoadAsync(object sender, RoutedEventArgs e)
		{
			ImageProcessing.OnLoadAsync(sender, e);
		}

		private void BrushThinkess_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			if (BrushStyleComboBox.SelectedItem == null)
			{
				return;
			}

			BrushStyle brushStyle = (BrushStyle)BrushStyleComboBox.SelectedItem;
			InkDrawingAttributes inkAttr = Brush.UpdateBrushSize(brushStyle, e.NewValue, e.NewValue);
			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(inkAttr);
		}

		private void TriggerPenColourChange(object sender, SelectionChangedEventArgs e)
		{
			if (BrushStyleComboBox.SelectedItem == null)
			{
				return;
			}

			BrushStyle brushStyle = (BrushStyle)BrushStyleComboBox.SelectedItem;
			ListView colourListView = sender as ListView;

			Color firstColour = new Color();
			Color? secondColour = null;

			if (colourListView.SelectedItems.Count == 1)
			{
				Rectangle selectedColour = colourListView.SelectedItem as Rectangle;
				SolidColorBrush color1 = selectedColour.Fill as SolidColorBrush;

				firstColour = color1.Color;
			}
			else if (colourListView.SelectedItems.Count > 1)
			{
				Rectangle firstSelectedColour = colourListView.SelectedItems[0] as Rectangle;
				Rectangle secondSelectedColour = colourListView.SelectedItems[1] as Rectangle;

				SolidColorBrush color1 = firstSelectedColour.Fill as SolidColorBrush;
				SolidColorBrush color2 = secondSelectedColour.Fill as SolidColorBrush;

				firstColour = color1.Color;
				secondColour = color2.Color;
			}

			InkDrawingAttributes inkAttr = Brush.UpdateBrushColour(firstColour, secondColour);

			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(inkAttr);
		}

		private void TriggerBrushTypeChanged(object sender, RoutedEventArgs e)
		{
			if (BrushStyleComboBox.SelectedItem == null)
			{
				return;
			}

			BrushStyle brushStyle = (BrushStyle)BrushStyleComboBox.SelectedItem;
			InkDrawingAttributes inkAttr = Brush.UpdateBrushType(brushStyle);

			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(inkAttr);
		}

		private void ErasingModeCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			InkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
		}

		private void ErasingModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			InkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
		}

		private void Hamburger_Click(object sender, RoutedEventArgs e)
		{
			Splitter.IsPaneOpen = (Splitter.IsPaneOpen != true);
		}

		//Recycle Bin functions
		private void RecycleBin_OnDrop(object sender, DragEventArgs e)
		{
			foreach (Rectangle obj in dragedColoursCollection)
			{
				brushColourCollection.Remove(obj);
			}

			if (dragedColoursCollection.Count > 0)
			{
				RecycleBin_OnDragLeave_Animation.Begin();
			}

			dragedColoursCollection.Clear();
		}
		private void RecycleBin_OnDragOver(object sender, DragEventArgs e)
		{
			if (dragedColoursCollection.Count > 0)
			{
				e.AcceptedOperation = DataPackageOperation.Move;
				e.DragUIOverride.Caption = string.Format(CultureInfo.CurrentCulture, ResourceLoader.GetForViewIndependentUse().GetString("TOOLTIP_REMOVE_COLOURS"), dragedColoursCollection.Count);
				e.DragUIOverride.IsContentVisible = false;
			}
			else
			{
				e.AcceptedOperation = DataPackageOperation.None;
			}
		}

		private void RecycleBin_OnDragEnter(object sender, DragEventArgs e)
		{
			RecycleBin_OnDragEnter_Animation.Begin();
		}

		private void RecycleBin_OnDragLeave(object sender, DragEventArgs e)
		{
			RecycleBin_OnDragLeave_Animation.Begin();
		}

		private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			foreach (object item in e.Items)
			{
				dragedColoursCollection.Add(item as Rectangle);
			}
		}

		private void ColourListViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			dragedColoursCollection.Clear();
		}

		//Custom Properties Functions
		private void CustomBrushSizeSliderOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			if (BrushStyleComboBox.SelectedItem == null)
			{
				return;
			}

			BrushStyle brushStyle = (BrushStyle)BrushStyleComboBox.SelectedItem;
			InkDrawingAttributes inkAttr = Brush.UpdateBrushSize(brushStyle, BrushHeightSlider.Value, BrushWidthSlider.Value);

			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(inkAttr);
			UpdateDrawingSettings();
		}

		private void CustomSettingsCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateDrawingSettings();
		}

		private void CustomPropertiesToggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			UpdateDrawingSettings();
		}

		private void CustomPropertiesHighliterMode_OnToggled(object sender, RoutedEventArgs e)
		{
			UpdateDrawingSettings();
		}

		private void AddColourFromPicker_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			brushColourCollection.Add(new Rectangle
			{
				Style = (Style)Application.Current.Resources["ColourOptionRectangle"],
				Fill = new SolidColorBrush(Color.FromArgb(255, (byte)NewColourPicker.RedValue, (byte)NewColourPicker.GreenValue, (byte)NewColourPicker.BlueValue))
			});

			ColourPalleteScrollViewer.ChangeView(0.0f, double.MaxValue, 1.0f);
		}

		private void TriggerBackgroundColourChanged(object sender, SelectionChangedEventArgs e)
		{
			//if(CanvasContainer == null)
			//{
			//	return;
			//}
			//switch (GridBackground_ComboBox.SelectedIndex)
			//{
			//	case 0:
			//		CanvasContainer.Background = new SolidColorBrush(Colors.White);
			//		break;
			//	case 1:
			//		CanvasContainer.Background = new SolidColorBrush(Colors.Black);
			//		break;
			//	case 2:
			//		CanvasContainer.Background = new SolidColorBrush(Colors.DimGray);
			//		break;
			//}
		}
		private void PressureSetting_OnToggle(object sender, RoutedEventArgs e)
		{
			if (PenPressureToggleSwitch == null)
			{
				return;
			}

			if (InkCanvas == null)
			{
				return;
			}

			InkDrawingAttributes drawingAttributes = InkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
			drawingAttributes.IgnorePressure = !PenPressureToggleSwitch.IsOn;
			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
		}

		//FullScreen Functions
		private void FullScreen_OnChecked(object sender, RoutedEventArgs e)
		{
			bool enteredFullScreenSuccesfully = ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

			if (enteredFullScreenSuccesfully)
			{
				ExpandToFullscreenToggleButton.Glyph = ShrinkFromFullScreen.ToString();
			}

		}
		private void FullScreen_OnUnChecked(object sender, RoutedEventArgs e)
		{
			ApplicationView.GetForCurrentView().ExitFullScreenMode();
			ExpandToFullscreenToggleButton.Glyph = ExpandToFullScreen.ToString();
		}

		private void PenSettingsButton_OnOpened(object sender, object e)
		{
			PenSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
		}

		private void PenSettingsButton_OnClosed(object sender, object e)
		{
			PenSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"];
		}

		private void LayersSettingsButton_OnOpened(object sender, object e)
		{
			LayerSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
		}

		private void LayersSettingsButton_OnClosed(object sender, object e)
		{
			LayerSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"];
		}

		private void SplitView_OnPaneClosed(SplitView sender, object args)
		{
			HamburgerToggleButton.IsChecked = false;
		}

		private void UpdateDrawingSettings()
		{
			if (BrushWidthSlider == null || BrushHeightSlider == null || CustomBrushStyle == null)
			{
				return;
			}

			if (CustomPropertiesHighlighterToggleSwitch == null)
			{
				return;
			}

			InkDrawingAttributes drawingAttributes = InkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
			if (CustomPropertiesToggleSwitch.IsOn)
			{
				BrushStyleComboBox.IsEnabled = false;
				BrushThinkessSlider.IsEnabled = false;
				drawingAttributes.Size = new Size(BrushWidthSlider.Value, BrushHeightSlider.Value);
				drawingAttributes.DrawAsHighlighter = CustomPropertiesHighlighterToggleSwitch.IsOn;

				double rotationRadians = BrushRotation_Slider.Value * Math.PI / 180;
				drawingAttributes.PenTipTransform = Matrix3x2.CreateRotation((float)rotationRadians);

				switch (CustomBrushStyle.SelectedIndex)
				{
					case 0:
						drawingAttributes.PenTip = PenTipShape.Rectangle;
						break;
					case 1:
						drawingAttributes.PenTip = PenTipShape.Circle;
						break;
				}
			}
			else
			{
				CustomPropertiesHighlighterToggleSwitch.IsOn = false;

				BrushStyleComboBox.IsEnabled = true;
				BrushThinkessSlider.IsEnabled = true;
			}

			Brush.DrawBrushSizePreview(BrushWidthSlider.Value, BrushHeightSlider.Value);
			InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
		}

		//Layer Functions
		private void NewLayerButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			LayerManager.GlobalInkLayers.Add(
				new InkLayer(true, new InkStrokeContainer(),
				string.Format(CultureInfo.CurrentCulture,
					ResourceLoader.GetForViewIndependentUse().GetString("LAYER_NAME_CONSTRUCTOR"),
					LayerManager.GlobalInkLayers.Count + 1)
				)
			);
		}
		private void DeleteLayerButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			foreach (object item in LayersListBox.SelectedItems)
			{
				LayerManager.GlobalInkLayers.Remove(item as InkLayer);
			}
		}
		private void DeleteLayerButton_OnDragEnter(object sender, DragEventArgs e)
		{
			DeleteLayer_OnDragOver_Animation.Begin();
		}

		private void DeleteLayerButton_OnDragLeave(object sender, DragEventArgs e)
		{
			DeleteLayer_OnDragLeave_Animation.Begin();
		}

		private void DeleteLayerButton_OnDragOver(object sender, DragEventArgs e)
		{
			if (dragedLayersCollection.Count > 0)
			{
				e.AcceptedOperation = DataPackageOperation.Move;
				e.DragUIOverride.Caption = string.Format(CultureInfo.CurrentCulture,
					ResourceLoader.GetForViewIndependentUse().GetString("TOOLTIP_REMOVE_LAYERS"),
					dragedLayersCollection.Count);
				e.DragUIOverride.IsContentVisible = false;
			}
			else
			{
				e.AcceptedOperation = DataPackageOperation.None;
			}
		}
		private void DeleteLayerButton_OnDrop(object sender, DragEventArgs e)
		{
			foreach (InkLayer inkLayer in dragedLayersCollection)
			{
				LayerManager.GlobalInkLayers.Remove(inkLayer);
			}

			if (dragedLayersCollection.Count > 0)
			{
				DeleteLayer_OnDragLeave_Animation.Begin();
			}

			dragedLayersCollection.Clear();
		}
		private void LockLayersToggleButtonChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			LockLayersFontIcon.Glyph = LayersLockedCharcter.ToString();
			LayersListBox.IsEnabled = false;
			LayerNewButton.IsEnabled = false;
			LayerDeleteButton.IsEnabled = false;
		}
		private void LockLayersToggleButtonUnChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			LockLayersFontIcon.Glyph = LayersUnLockedCharcter.ToString();
			LayersListBox.IsEnabled = true;
			LayerNewButton.IsEnabled = true;
			LayerDeleteButton.IsEnabled = true;
		}
		private void LayersListView_OnDragStarting(object o, DragItemsStartingEventArgs e)
		{
			foreach (object item in e.Items)
			{
				dragedLayersCollection.Add(item as InkLayer);
			}
		}
		private void LayersViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			dragedLayersCollection.Clear();
		}

		private void LayersView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				InkLayer layer = LayersListBox.SelectedItems[0] as InkLayer;
				if (layer != null)
				{
					InkCanvas.InkPresenter.StrokeContainer = layer.LayerStrokeContainer;
				}
			}
			catch (Exception)
			{

				//g
			}

		}
	}

}
