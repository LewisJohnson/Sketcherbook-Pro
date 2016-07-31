using Windows.System;
using Windows.UI.Core;

namespace SketcherBook_Pro.Helpers
{
    public sealed class KeyboardShortcuts
    {
        private readonly MainPage _mainPage;
        private readonly Brush _brush;

        public KeyboardShortcuts(MainPage mainPage)
        {
            _mainPage = mainPage;
            _brush = new Brush(mainPage);
        }

        public void Global_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case VirtualKey.W:
                    _mainPage.BrushThinkess_Slider.Value += 5;
                    _brush.DrawBrushSizePreview(_mainPage.BrushThinkess_Slider.Value, false);
                    break;

                case VirtualKey.S:
                    _mainPage.BrushThinkess_Slider.Value -= 5;
                    _brush.DrawBrushSizePreview(_mainPage.BrushThinkess_Slider.Value, false);
                    break;

            }

        }
    }
}
