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
using Brush = SketcherBook_Pro.Helpers.Brush;

namespace SketcherBook_Pro
{

	public sealed partial class MainPage
	{
		public ImageProcessing ImageProcessing { get; set; }
		public Brush Brush { get; set; }
		public ShareImage ShareImage { get; set; }
		public static KeyboardShortcuts KeyboardShortcuts { get; private set; }
		public double PenThickness;

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
			ImageProcessing = new ImageProcessing(this);
			KeyboardShortcuts = new KeyboardShortcuts(this);
			Brush = new Brush(this);
			ShareImage = new ShareImage(this);
			InitializeComponent();

			ObservableCollection<SolidColorBrush> brushColourCollection = Initialisers.PopulateInitialColourCollection();

			foreach (SolidColorBrush item in brushColourCollection)
			{
				this.brushColourCollection.Add(new Rectangle
				{
					Style = (Style)Application.Current.Resources["ColourOptionRectangle"],
					Fill = item
				});
			}
			this.InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(Initialisers.InitialDrawingAttributes());
			this.InkCanvas_Main.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen |
														   CoreInputDeviceTypes.Touch;

		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			this.IntroAnimation.Begin();
			PopulateElements();
			SetTriggers();
		}

		private void PopulateElements()
		{
			this.InkCanvas_Main.InkPresenter.StrokeContainer = LayerManager.GlobalInkLayers.First().LayerStrokeContainer;
			this.ColourListView.ItemsSource = this.brushColourCollection;
			this.LayersListBox.ItemsSource = LayerManager.GlobalInkLayers;
		}

		private void SetTriggers()
		{
			this.ShareFacebook.Tapped += ShareImage.ShareFacebook;
			this.ShareTwitter.Tapped += ShareImage.ShareTwitter;
			this.ShareGoogle.Tapped += ShareImage.ShareGoogle;

			this.InkCanvas_Main.InkPresenter.StrokesCollected += InkPresenterOnStrokesCollected;
			this.InkCanvas_Main.InkPresenter.StrokesErased += InkPresenterOnStrokesErased;
		}

		private void OnShare(object sender, RoutedEventArgs e)
		{
			this.SharePopUp.IsOpen = this.SharePopUp.IsOpen == false;
		}

		private void ShareCloseButton(object sender, TappedRoutedEventArgs e)
		{
			this.SharePopUp.IsOpen = false;
		}


		//Undo Functions
		private void InkPresenterOnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
		{
			foreach (InkStroke item in args.Strokes)
			{
				this.cachedInkStrokesCollection.Add(new InkAction(item, InkActionType.Erased));
			}
		}
		private void InkPresenterOnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
		{
			foreach (InkStroke item in args.Strokes)
			{
				this.cachedInkStrokesCollection.Add(new InkAction(item, InkActionType.Collected));
			}

		}
		private void UndoButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			InkAction item = this.cachedInkStrokesCollection.Last();
			if (item.Action == InkActionType.Collected)
			{
				throw new NotImplementedException();
			}
			else if (item.Action == InkActionType.Erased)
			{
				throw new NotImplementedException();
			}
		}

		private void OnClear(object sender, RoutedEventArgs e)
		{
			this.InkCanvas_Main.InkPresenter.StrokeContainer.Clear();
		}

		private void OnSaveAsync(object sender, RoutedEventArgs e)
		{
			ImageProcessing.OnSaveAsync(sender, e);
		}

		private void OnLoadAsync(object sender, RoutedEventArgs e)
		{
			ImageProcessing.OnLoadAsync(sender, e);
		}

		private void BrushThinkess_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			Brush.BrushThicknessChanger();
		}

		private void TriggerPenColourChange(object sender, SelectionChangedEventArgs e)
		{
			Brush.TriggerPenColourChange(sender, e);
		}

		private void TriggerPenTypeChanged(object sender, RoutedEventArgs e)
		{
			Brush.TriggerPenTypeChanged();
		}

		private void ErasingModeCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			this.InkCanvas_Main.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
		}

		private void ErasingModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			this.InkCanvas_Main.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
		}

		private void Hamburger_Click(object sender, RoutedEventArgs e)
		{
			this.Splitter.IsPaneOpen = (this.Splitter.IsPaneOpen != true);
		}

		//Recycle Bin functions
		private void RecycleBin_OnDrop(object sender, DragEventArgs e)
		{
			foreach (Rectangle obj in this.dragedColoursCollection)
			{
				this.brushColourCollection.Remove(obj);
			}

			if (this.dragedColoursCollection.Count > 0)
			{
				this.RecycleBin_OnDragLeave_Animation.Begin();
			}

			this.dragedColoursCollection.Clear();
		}
		private void RecycleBin_OnDragOver(object sender, DragEventArgs e)
		{
			if (this.dragedColoursCollection.Count > 0)
			{
				e.AcceptedOperation = DataPackageOperation.Move;
				e.DragUIOverride.Caption = string.Format(CultureInfo.CurrentCulture, ResourceLoader.GetForViewIndependentUse().GetString("TOOLTIP_REMOVE_COLOURS"), this.dragedColoursCollection.Count);
				e.DragUIOverride.IsContentVisible = false;
			}
			else
			{
				e.AcceptedOperation = DataPackageOperation.None;
			}
		}

		private void RecycleBin_OnDragEnter(object sender, DragEventArgs e)
		{
			this.RecycleBin_OnDragEnter_Animation.Begin();
		}

		private void RecycleBin_OnDragLeave(object sender, DragEventArgs e)
		{
			this.RecycleBin_OnDragLeave_Animation.Begin();
		}

		private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			foreach (object item in e.Items)
			{
				this.dragedColoursCollection.Add(item as Rectangle);
			}
		}

		private void ColourListViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			this.dragedColoursCollection.Clear();
		}

		//Custom Properties Functions
		private void CustomeSettingSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
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
			this.brushColourCollection.Add(new Rectangle
			{
				Style = (Style)Application.Current.Resources["ColourOptionRectangle"],
				Fill = new SolidColorBrush(Color.FromArgb(255, (byte)this.NewColourPicker.RedValue, (byte)this.NewColourPicker.GreenValue, (byte)this.NewColourPicker.BlueValue))
			});

			this.ColourPalleteScrollViewer.ChangeView(0.0f, double.MaxValue, 1.0f);
		}

		private void TriggerBackgroundColourChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.InkGrid == null)
			{
				return;
			}

			switch (this.GridBackground_ComboBox.SelectedIndex)
			{
				case 0:
					this.InkGrid.Background = new SolidColorBrush(Colors.White);
					break;
				case 1:
					this.InkGrid.Background = new SolidColorBrush(Colors.Black);
					break;
			}
		}
		private void PressureSetting_OnToggle(object sender, RoutedEventArgs e)
		{
			if (this.PenPressureToggleSwitch == null)
			{
				return;
			}

			if (this.InkCanvas_Main == null)
			{
				return;
			}

			InkDrawingAttributes drawingAttributes = this.InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
			drawingAttributes.IgnorePressure = !this.PenPressureToggleSwitch.IsOn;
			this.InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
		}

		//FullScreen Functions
		private void FullScreen_OnChecked(object sender, RoutedEventArgs e)
		{
			bool enteredFullScreenSuccesfully = ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

			if (enteredFullScreenSuccesfully)
			{
				this.ExpandToFullscreenToggleButton.Glyph = ShrinkFromFullScreen.ToString();
			}
			
		}
		private void FullScreen_OnUnChecked(object sender, RoutedEventArgs e)
		{
			ApplicationView.GetForCurrentView().ExitFullScreenMode();
			this.ExpandToFullscreenToggleButton.Glyph = ExpandToFullScreen.ToString();
		}

		private void PenSettingsButton_OnOpened(object sender, object e)
		{
			this.PenSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
		}

		private void PenSettingsButton_OnClosed(object sender, object e)
		{
			this.PenSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"];
		}

		private void LayersSettingsButton_OnOpened(object sender, object e)
		{
			this.LayerSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
		}

		private void LayersSettingsButton_OnClosed(object sender, object e)
		{
			this.LayerSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"];
		}

		private void SplitView_OnPaneClosed(SplitView sender, object args)
		{
			this.HamburgerToggleButton.IsChecked = false;
		}

		private void UpdateDrawingSettings()
		{
			if (this.BrushWidthSlider == null || this.BrushHeightSlider == null || this.CustomBrushStyle == null)
			{
				return;
			}

			if (this.CustomPropertiesHighlighterToggleSwitch == null)
			{
				return;
			}

			InkDrawingAttributes drawingAttributes = this.InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
			if (this.CustomPropertiesToggleSwitch.IsOn)
			{
				this.BrushStyleComboBox.IsEnabled = false;
				this.BrushThinkessSlider.IsEnabled = false;
				drawingAttributes.Size = new Size(this.BrushWidthSlider.Value, this.BrushHeightSlider.Value);
				drawingAttributes.DrawAsHighlighter = this.CustomPropertiesHighlighterToggleSwitch.IsOn;

				double rotationRadians = this.BrushRotation_Slider.Value * Math.PI / 180;
				drawingAttributes.PenTipTransform = Matrix3x2.CreateRotation((float)rotationRadians);

				switch (this.CustomBrushStyle.SelectedIndex)
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
				this.CustomPropertiesHighlighterToggleSwitch.IsOn = false;

				this.BrushStyleComboBox.IsEnabled = true;
				this.BrushThinkessSlider.IsEnabled = true;
			}

			Brush.DrawBrushSizePreview(this.BrushWidthSlider.Value, this.BrushHeightSlider.Value);
			this.InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
		}

		//Layer Functions
		private void NewLayerButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			LayerManager.GlobalInkLayers.Add(new InkLayer(true, new InkStrokeContainer(), string.Format(CultureInfo.CurrentCulture,
						ResourceLoader.GetForViewIndependentUse().GetString("LAYER_NAME_CONSTRUCTOR"), LayerManager.GlobalInkLayers.Count + 1)));
		}
		private void DeleteLayerButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			foreach (object item in this.LayersListBox.SelectedItems)
			{
				LayerManager.GlobalInkLayers.Remove(item as InkLayer);
			}
		}
		private void DeleteLayerButton_OnDragEnter(object sender, DragEventArgs e)
		{
			this.DeleteLayer_OnDragOver_Animation.Begin();
		}

		private void DeleteLayerButton_OnDragLeave(object sender, DragEventArgs e)
		{
			this.DeleteLayer_OnDragLeave_Animation.Begin();
		}

		private void DeleteLayerButton_OnDragOver(object sender, DragEventArgs e)
		{
			if (this.dragedLayersCollection.Count > 0)
			{
				e.AcceptedOperation = DataPackageOperation.Move;
				e.DragUIOverride.Caption = string.Format(CultureInfo.CurrentCulture,
					ResourceLoader.GetForViewIndependentUse().GetString("TOOLTIP_REMOVE_LAYERS"),
					this.dragedLayersCollection.Count);
				e.DragUIOverride.IsContentVisible = false;
			}
			else
			{
				e.AcceptedOperation = DataPackageOperation.None;
			}
		}
		private void DeleteLayerButton_OnDrop(object sender, DragEventArgs e)
		{
			foreach (InkLayer inkLayer in this.dragedLayersCollection)
			{
				LayerManager.GlobalInkLayers.Remove(inkLayer);
			}

			if (this.dragedLayersCollection.Count > 0)
			{
				this.DeleteLayer_OnDragLeave_Animation.Begin();
			}

			this.dragedLayersCollection.Clear();
		}
		private void LockLayersToggleButtonChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			this.LockLayersFontIcon.Glyph = LayersLockedCharcter.ToString();
			this.LayersListBox.IsEnabled = false;
			this.LayerNewButton.IsEnabled = false;
			this.LayerDeleteButton.IsEnabled = false;
		}
		private void LockLayersToggleButtonUnChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			this.LockLayersFontIcon.Glyph = LayersUnLockedCharcter.ToString();
			this.LayersListBox.IsEnabled = true;
			this.LayerNewButton.IsEnabled = true;
			this.LayerDeleteButton.IsEnabled = true;
		}
		private void LayersListView_OnDragStarting(object o, DragItemsStartingEventArgs e)
		{
			foreach (object item in e.Items)
			{
				this.dragedLayersCollection.Add(item as InkLayer);
			}
		}
		private void LayersViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			this.dragedLayersCollection.Clear();
		}

		private void LayersView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				InkLayer layer = this.LayersListBox.SelectedItems[0] as InkLayer;
				if (layer != null)
				{
					this.InkCanvas_Main.InkPresenter.StrokeContainer = layer.LayerStrokeContainer;
				}
			}
			catch (Exception)
			{

				//g
			}

		}
	}

}
