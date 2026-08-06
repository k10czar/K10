using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    public interface IExplorerTabBuilder
    {
        public void Rebuild(IExplorerWindow window, VisualElement root);
        public void Minimized() {}
        public void TabClosed() {}
        public void WindowClosed() {}
    }
}