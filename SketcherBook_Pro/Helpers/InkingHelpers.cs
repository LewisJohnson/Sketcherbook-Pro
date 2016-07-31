using Windows.UI.Input.Inking;

namespace SketcherBook_Pro.Helpers
{

    public class InkAction
    {
        public InkActionType Action { get; set; }
        public InkStroke Stroke { get; set; }

        public InkAction(InkStroke stroke, InkActionType action)
        {
            Action = action;
            Stroke = stroke;
        }
    }

    public enum InkActionType
    {
        Collected,
        Erased
    }
}
