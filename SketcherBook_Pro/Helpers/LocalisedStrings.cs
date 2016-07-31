using Windows.ApplicationModel.Resources;

namespace SketcherBook_Pro.Helpers
{
    class LocalisedStrings
    {
        public string this[string key] => ResourceLoader.GetForViewIndependentUse().GetString(key);
    }
}
