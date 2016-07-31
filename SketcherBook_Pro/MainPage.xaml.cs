using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.ServiceModel.Description;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using SketcherBook_Pro.Helpers;
using Brush = SketcherBook_Pro.Helpers.Brush;
using Windows.UI.Xaml.Media.Animation;

namespace SketcherBook_Pro
{

    public sealed partial class MainPage
    {
        public ImageProcessing ImageProcessing { get; set; }
        public Brush Brush { get; set; }
        public ShareImage ShareImage { get; set; }
        public static KeyboardShortcuts KeyboardShortcuts { get; private set; }
        public double PenThickness;

        private readonly ObservableCollection<InkAction> _cachedInkStrokesCollection = new ObservableCollection<InkAction>();
        private readonly ObservableCollection<Rectangle> _brushColourCollection = new ObservableCollection<Rectangle>();
        private readonly ICollection<Rectangle> _dragedColoursCollection = new List<Rectangle>();
        private readonly ICollection<InkLayer> _dragedLayersCollection = new List<InkLayer>();
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


            var brushColourCollection = Initialisers.PopulateInitialColourCollection();

            foreach (var item in brushColourCollection)
            {
                _brushColourCollection.Add(new Rectangle
                {
                    Style = (Style) Application.Current.Resources["ColourOptionRectangle"],
                    Fill = item
                });
            }
            InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(Initialisers.InitialDrawingAttributes());
            InkCanvas_Main.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen |
                                                           CoreInputDeviceTypes.Touch;

        }
        
        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Intro_Animation.Begin();
            PopulateElements();
            SetTriggers();
        }

        private void PopulateElements()
        {
            InkCanvas_Main.InkPresenter.StrokeContainer = LayerManager.GlobalInkLayers.First().LayerStrokeContainer;
            ColourListView.ItemsSource = _brushColourCollection;
            LayersListBox.ItemsSource = LayerManager.GlobalInkLayers;
        }

        private void SetTriggers()
        {
            ShareFacebook.Tapped += ShareImage.ShareFacebook;
            ShareTwitter.Tapped += ShareImage.ShareTwitter;
            ShareGoogle.Tapped += ShareImage.ShareGoogle;

            InkCanvas_Main.InkPresenter.StrokesCollected += InkPresenterOnStrokesCollected;
            InkCanvas_Main.InkPresenter.StrokesErased += InkPresenterOnStrokesErased;
        }

        private void OnShare(object sender, RoutedEventArgs e) => SharePopUp.IsOpen = SharePopUp.IsOpen == false;
        private void ShareCloseButton(object sender, TappedRoutedEventArgs e) => SharePopUp.IsOpen = false;


        //Undo Functions
        private void InkPresenterOnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            foreach (var item in args.Strokes)
            {
                _cachedInkStrokesCollection.Add(new InkAction(item, InkActionType.Erased));
            }
        }
        private void InkPresenterOnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var item in args.Strokes)
            {
                _cachedInkStrokesCollection.Add(new InkAction(item, InkActionType.Collected));
            }

        }
        private void UndoButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var item = _cachedInkStrokesCollection.Last();
            if (item.Action == InkActionType.Collected)
            {
                throw new NotImplementedException();
            }
            else if(item.Action == InkActionType.Erased)
            {
                throw new NotImplementedException();
            }
        }

        private void OnClear(object sender, RoutedEventArgs e) => InkCanvas_Main.InkPresenter.StrokeContainer.Clear();
        private void OnSaveAsync(object sender, RoutedEventArgs e) => ImageProcessing.OnSaveAsync(sender, e);
        private void OnLoadAsync(object sender, RoutedEventArgs e) => ImageProcessing.OnLoadAsync(sender, e);
        
        private void BrushThinkess_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => Brush.BrushThicknessChanger();
        private void TriggerPenColourChange(object sender, SelectionChangedEventArgs e) => Brush.TriggerPenColourChange(sender, e);
        private void TriggerPenTypeChanged(object sender, RoutedEventArgs e) => Brush.TriggerPenTypeChanged();
        private void ErasingModeCheckBox_Checked(object sender, RoutedEventArgs e) => InkCanvas_Main.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
        private void ErasingModeCheckBox_Unchecked(object sender, RoutedEventArgs e) => InkCanvas_Main.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
        private void Hamburger_Click(object sender, RoutedEventArgs e) => Splitter.IsPaneOpen = (Splitter.IsPaneOpen != true);

        //Recycle Bin functions
        private void RecycleBin_OnDrop(object sender, DragEventArgs e)
        {
            foreach (var obj in _dragedColoursCollection)
            {
                _brushColourCollection.Remove(obj);
            }

            if (_dragedColoursCollection.Count > 0)
            {
                RecycleBin_OnDragLeave_Animation.Begin();
            }

            _dragedColoursCollection.Clear();;
        }
        private void RecycleBin_OnDragOver(object sender, DragEventArgs e)
        {
            if (_dragedColoursCollection.Count > 0)
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.Caption = string.Format(CultureInfo.CurrentCulture,
                    ResourceLoader.GetForViewIndependentUse().GetString("TOOLTIP_REMOVE_COLOURS"),
                    _dragedColoursCollection.Count);
                e.DragUIOverride.IsContentVisible = false;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }

        }
        private void RecycleBin_OnDragEnter(object sender, DragEventArgs e) => RecycleBin_OnDragEnter_Animation.Begin();
        private void RecycleBin_OnDragLeave(object sender, DragEventArgs e) => RecycleBin_OnDragLeave_Animation.Begin();
        private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            foreach (var item in e.Items)
            {
                _dragedColoursCollection.Add(item as Rectangle);
            }
        }
        private void ColourListViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args) => _dragedColoursCollection.Clear();

        //Custom Properties Functions
        private void CustomeSettingSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e) => UpdateDrawingSettings();
        private void CustomSettingsCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDrawingSettings();
        private void CustomPropertiesToggleSwitch_Toggled(object sender, RoutedEventArgs e) => UpdateDrawingSettings();
        private void CustomPropertiesHighliterMode_OnToggled(object sender, RoutedEventArgs e) => UpdateDrawingSettings();

        private void AddColourFromPicker_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _brushColourCollection.Add(new Rectangle
            {
                Style = (Style)Application.Current.Resources["ColourOptionRectangle"],
                Fill = new SolidColorBrush(Color.FromArgb(255, (byte)NewColourPicker.RedValue, (byte)NewColourPicker.GreenValue, (byte)NewColourPicker.BlueValue))
            });

            ColourPalleteScrollViewer.ChangeView(0.0f, double.MaxValue, 1.0f);
        }
        private void RightPanelExpandButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (RightPanelExpandButton.IsChecked == true)
            {
                RightPanelColumnDefinition.Width = new GridLength(75);
                RightPanelExpandButton.Content = ShrinkPanelCharcter;
                RightPanelShow_Animation.Begin();

            }
            else
            {
                RightPanelExpandButton.Content = ExpandPanelCharcter;
                RightPanelHide_Animation.Begin();
                RightPanelHide_Animation.Completed += (o, o1) =>
                {
                    RightPanelColumnDefinition.Width = new GridLength(0);
                };
            }
        }
        private void TriggerBackgroundColourChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InkGrid == null) return;
            switch (GridBackground_ComboBox.SelectedIndex)
            {
                case 0:
                    InkGrid.Background = new SolidColorBrush(Colors.White);
                    break;
                case 1:
                    InkGrid.Background = new SolidColorBrush(Colors.Black);
                    break;
            }
        }
        private void PressureSetting_OnToggle(object sender, RoutedEventArgs e)
        {
            if (PenPressureToggleSwitch == null) return;
            if (InkCanvas_Main == null) return;
            var drawingAttributes = InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
            drawingAttributes.IgnorePressure = !PenPressureToggleSwitch.IsOn;
            InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
        }

        //FullScreen Functions
        private void FullScreen_OnChecked(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            ExpandToFullscreenToggleButton.Glyph = ShrinkFromFullScreen.ToString();
            RightPanelExpandButton.Visibility = Visibility.Visible;

        }
        private void FullScreen_OnUnChecked(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            ExpandToFullscreenToggleButton.Glyph = ExpandToFullScreen.ToString();
            RightPanelExpandButton.Visibility = Visibility.Collapsed;

            RightPanelColumnDefinition.Width = new GridLength(75);
            RightPanelExpandButton.Content = ShrinkPanelCharcter;
            RightPanelShow_Animation.Begin();
        }

        private void PenSettingsButton_OnOpened(object sender, object e) => PenSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
        private void PenSettingsButton_OnClosed(object sender, object e) => PenSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"];
        private void LayersSettingsButton_OnOpened(object sender, object e) => LayerSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
        private void LayersSettingsButton_OnClosed(object sender, object e) => LayerSettingsButton.Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"];
        private void SplitView_OnPaneClosed(SplitView sender, object args) => HamburgerToggleButton.IsChecked = false;

        private void UpdateDrawingSettings()
        {
            if (BrushWidth_Slider == null) return;
            if (BrushHeight_Slider == null) return;
            if (CustomBrushStyle == null) return;
            if (CustomPropertiesHighlighterToggleSwitch == null) return;

            var drawingAttributes = InkCanvas_Main.InkPresenter.CopyDefaultDrawingAttributes();
            if (CustomPropertiesToggleSwitch.IsOn)
            {
                BrushStyle_ComboBox.IsEnabled = false;
                BrushThinkess_Slider.IsEnabled = false;
                drawingAttributes.Size = new Size(BrushWidth_Slider.Value, BrushHeight_Slider.Value);
                drawingAttributes.DrawAsHighlighter = CustomPropertiesHighlighterToggleSwitch.IsOn;

                var rotationRadians = BrushRotation_Slider.Value * Math.PI / 180;
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

                BrushStyle_ComboBox.IsEnabled = true;
                BrushThinkess_Slider.IsEnabled = true;
            }

            Brush.DrawBrushSizePreview(BrushWidth_Slider.Value, BrushHeight_Slider.Value, true);
            InkCanvas_Main.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
        }

        //Layer Functions
        private void NewLayerButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            LayerManager.GlobalInkLayers.Add(new InkLayer(true, new InkStrokeContainer(), string.Format(CultureInfo.CurrentCulture,
                        ResourceLoader.GetForViewIndependentUse().GetString("LAYER_NAME_CONSTRUCTOR"), LayerManager.GlobalInkLayers.Count + 1)));
        }
        private void DeleteLayerButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (var item in LayersListBox.SelectedItems)
            {
                LayerManager.GlobalInkLayers.Remove(item as InkLayer);
            }
        }
        private void DeleteLayerButton_OnDragEnter(object sender, DragEventArgs e) => DeleteLayer_OnDragOver_Animation.Begin();
        private void DeleteLayerButton_OnDragLeave(object sender, DragEventArgs e) => DeleteLayer_OnDragLeave_Animation.Begin();
        private void DeleteLayerButton_OnDragOver(object sender, DragEventArgs e)
        {
            if (_dragedLayersCollection.Count > 0)
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.Caption = string.Format(CultureInfo.CurrentCulture,
                    ResourceLoader.GetForViewIndependentUse().GetString("TOOLTIP_REMOVE_LAYERS"),
                    _dragedLayersCollection.Count);
                e.DragUIOverride.IsContentVisible = false;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        private void DeleteLayerButton_OnDrop(object sender, DragEventArgs e)
        {
            foreach (var obj in _dragedLayersCollection)
            {
                LayerManager.GlobalInkLayers.Remove(obj);
            }

            if (_dragedLayersCollection.Count > 0)
            {
                DeleteLayer_OnDragLeave_Animation.Begin();
            }

            _dragedLayersCollection.Clear();
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
            foreach (var item in e.Items)
            {
                _dragedLayersCollection.Add(item as InkLayer);
            }
        }
        private void LayersViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args) => _dragedLayersCollection.Clear();
        private void LayersView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var layer = LayersListBox.SelectedItems[0] as InkLayer;
                if (layer != null) InkCanvas_Main.InkPresenter.StrokeContainer = layer.LayerStrokeContainer;
            }
            catch (Exception)
            {
                
                //g
            }

        }
    }

}
