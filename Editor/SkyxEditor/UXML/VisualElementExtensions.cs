using UnityEngine.UIElements;

namespace Rogue.REditor
{
    public static class VisualElementExtensions
    {
        public static void SetVisible(this VisualElement element, bool isVisible)
            => element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        public static void ToggleClassList(this VisualElement element, string className, bool shouldAdd)
        {
            if (shouldAdd) element.AddToClassList(className);
            else element.RemoveFromClassList(className);
        }
    }
}