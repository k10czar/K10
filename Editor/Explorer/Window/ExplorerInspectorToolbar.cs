using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    [UxmlElement]
    public partial class ExplorerInspectorToolbar : Toolbar
    {
        private ExplorerEntryOptions options;
        private ToolbarBreadcrumbs breadcrumbs;

        public void Setup(IExplorerWindow window, ExplorerInspectorInfo info)
        {
            SetRefs();

            var entryDef = ExplorerEditorLib.CreateExplorerEntry(info, true);
            options.SetOptions(entryDef);

            var hasBreadcrumbs = info.breadcrumbs is { Count: > 0 };
            breadcrumbs.style.display = hasBreadcrumbs ? DisplayStyle.Flex : DisplayStyle.None;

            if (hasBreadcrumbs)
            {
                breadcrumbs.Clear();
                foreach (var entry in info.breadcrumbs)
                {
                    var entryName = ExplorerEditorLib.GetDisplayName(entry, string.Empty);
                    breadcrumbs.PushItem(entryName, () => ExplorerEditorLib.OpenOrHighlightInspector(window, entry, string.Empty, null));
                }
            }
        }

        private void SetRefs()
        {
            if (options != null) return;

            options = this.Q<ExplorerEntryOptions>("EntryOptions");
            breadcrumbs = this.Q<ToolbarBreadcrumbs>("Breadcrumbs");
        }
    }
}