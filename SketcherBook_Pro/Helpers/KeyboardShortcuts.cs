using Windows.System;
using Windows.UI.Core;

namespace SketcherBook_Pro.Helpers
{
    public sealed class KeyboardShortcuts
    {
        private readonly CanvasBrush brush;

        public KeyboardShortcuts()
        {
            brush = new CanvasBrush();
        }

        public void Global_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case VirtualKey.W:
					brush.AdjustBrushSize(5);
                    break;

                case VirtualKey.S:
					brush.AdjustBrushSize(-5);
                    break;

            }
        }
    }
}
