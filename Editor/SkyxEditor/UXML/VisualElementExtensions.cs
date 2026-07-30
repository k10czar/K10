using UnityEngine.UIElements;

namespace Rogue.REditor
{
    public static class VisualElementExtensions
    {
        public static void SetVisible(this VisualElement element, bool isVisible)
            => element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}