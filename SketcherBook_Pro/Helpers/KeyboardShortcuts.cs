using Windows.System;
using Windows.UI.Core;

namespace SketcherBook_Pro.Helpers
{
    public sealed class KeyboardShortcuts
    {
        private readonly MainPage mainPage;
        private readonly Brush brush;

        public KeyboardShortcuts(MainPage mainPage)
        {
            this.mainPage = mainPage;
            brush = new Brush(mainPage);
        }

        public void Global_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case VirtualKey.W:
                    mainPage.BrushThinkessSlider.Value += 5;
                    brush.DrawBrushSizePreview(mainPage.BrushThinkessSlider.Value, mainPage.BrushThinkessSlider.Value);
                    break;

                case VirtualKey.S:
                    mainPage.BrushThinkessSlider.Value -= 5;
                    brush.DrawBrushSizePreview(mainPage.BrushThinkessSlider.Value, mainPage.BrushThinkessSlider.Value);
                    break;

            }
        }
    }
}
