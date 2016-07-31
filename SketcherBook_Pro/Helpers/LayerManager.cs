using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Input.Inking;

namespace SketcherBook_Pro.Helpers
{
    static class LayerManager
    {
        public static ObservableCollection<InkLayer> GlobalInkLayers { get; set; } = new ObservableCollection<InkLayer>();

        static LayerManager()
        {
            GlobalInkLayers.CollectionChanged += GlobalInkLayersOnCollectionChanged;

            for (var i = 1; i < 4; i++)
            {
                GlobalInkLayers.Add(new InkLayer(true, new InkStrokeContainer(),
                    string.Format(CultureInfo.CurrentCulture,
                        ResourceLoader.GetForViewIndependentUse().GetString("LAYER_NAME_CONSTRUCTOR"), i)));
            }
        }

        private static void GlobalInkLayersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddLayer();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveLayer();
                    break;
            }
        }

        private static void RemoveLayer()
        {
            
        }

        private static void AddLayer()
        {

        }

        private static void ShowLayer()
        {

        }

        private static void HideLayer()
        {

        }
    }

    public class InkLayer
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public InkStrokeContainer LayerStrokeContainer { get; set; }

        public InkLayer(bool isVisible, InkStrokeContainer layerStrokeContainer, string name)
        {
            IsVisible = isVisible;
            LayerStrokeContainer = layerStrokeContainer;
            Name = name;
        }
    }
}
