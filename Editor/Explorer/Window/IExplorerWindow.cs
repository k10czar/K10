using UnityEngine;

namespace Rogue.Explorer
{
    public interface IExplorerWindow
    {
        public ExplorerTabsPanel TabsPanel { get; }
        public ExplorerInspectors Inspectors { get; }

        public Object ExplorerOwner { get; }
        public ExplorerEditorSaveEntry ExplorerOwnerSaveData => ExplorerEditorConfig.GetExplorerSave(ExplorerOwner);

        void Rebuild(bool changedOwner);
    }
}