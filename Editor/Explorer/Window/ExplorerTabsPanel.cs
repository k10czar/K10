using UnityEngine;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    [UxmlElement]
    public partial class ExplorerTabsPanel : VisualElement
    {
        public IExplorerWindow Window { get; private set; }

        private VisualElement explorerContent;
        private Foldout favorites;
        private TabView tabView;
        private Tab selectedTab;

        #region Tabs Manipulation

        public bool HasActiveTab => selectedTab is { dataSource: not null };

        public void CreateOrSelectTab(string tabID, string label, IExplorerTabBuilder builder, bool isCloseable = true, bool selectTab = true)
        {
            var tab = FindTab(tabID);

            if (tab == null)
            {
                tab = new Tab
                {
                    name = tabID,
                    label = label,
                    closeable = isCloseable,
                    dataSource = builder,
                };
                tabView.Add(tab);
            }

            if (selectTab) SelectTab(tab);
        }

        public bool TryOpenTab(string tabID)
        {
            var tab = FindTab(tabID);
            if (tab == null) return false;

            SelectTab(tab);
            return true;
        }

        public Tab FindTab(string tabID)
        {
            var allTabs = tabView.Query<Tab>().ToList();
            foreach (var tab in allTabs)
            {
                if (tab.name != tabID) continue;

                return tab;
            }

            return null;
        }

        public bool HasTab(string tabID) => FindTab(tabID) != null;

        public void SelectTab(string tabID) => SelectTab(FindTab(tabID));

        public void SelectTab(Tab tab)
        {
            if (tab == null)
            {
                Debug.LogError("Trying to select a null tab!");
                return;
            }

            tabView.activeTab = tab;
        }

        private void OnTabChanged(Tab oldTab, Tab newTab)
        {
            (oldTab?.dataSource as IExplorerTabBuilder)?.Minimized();

            selectedTab = newTab;
            RebuildContent(false);
        }

        private void OnTabClosed(Tab closedTab, int closedIndex)
        {
            (closedTab.dataSource as IExplorerTabBuilder)?.TabClosed();

            var allTabs = tabView.Query<Tab>().ToList();
            if (allTabs.Count == 0)
            {
                selectedTab = null;
                RebuildContent(false);
            }
        }

        #endregion

        #region Build / Dispose

        public void SetWindow(IExplorerWindow windowRef) => Window = windowRef;

        private void SetRefs()
        {
            if (explorerContent != null) return;

            favorites = this.Q<Foldout>("Favorites");
            explorerContent = this.Q<VisualElement>("explorer-content");
            tabView = this.Q<TabView>("TabView");
            tabView.activeTabChanged += OnTabChanged;
            tabView.tabClosed += OnTabClosed;
        }

        public void RebuildFavorites()
        {
            var saveData = Window.ExplorerOwnerSaveData;
            var root = favorites.Q<VisualElement>("unity-content");
            root.Clear();

            if (saveData.favorites.Count == 0)
            {
                favorites.style.display = DisplayStyle.None;
                return;
            }
            favorites.style.display = DisplayStyle.Flex;

            foreach (var (target, propertyPath) in saveData.favorites)
            {
                var mainEntry = ExplorerEntryView.Create(Window, target, propertyPath, null);
                root.Add(mainEntry);
            }
        }

        private void OpenSearches()
        {
            var saveData = Window.ExplorerOwnerSaveData;

            foreach (var entry in saveData.searches)
            {
                if (HasTab(entry.tabID)) continue;

                var tabBuilder = new ExplorerSearchTabBuilder(Window, entry, false);
                CreateOrSelectTab(entry.tabID, entry.name, tabBuilder);
            }
        }

        public void RebuildContent(bool isRebuildingAll)
        {
            ExplorerEntryView.isOddEntry = false;

            SetRefs();
            RebuildFavorites();

            if (isRebuildingAll)
            {
                var allTabs = tabView.Query<Tab>().ToList();
                for (var i = allTabs.Count - 1; i >= 0; i--)
                    tabView.RemoveAt(i);

                OpenSearches();
            }

            explorerContent.Clear();
            (selectedTab?.dataSource as IExplorerTabBuilder)?.Rebuild(Window, explorerContent);
        }

        public void WindowClosed()
        {
            var allTabs = tabView.Query<Tab>().ToList();
            foreach (var tab in allTabs)
                (tab.dataSource as IExplorerTabBuilder)?.WindowClosed();
        }

        #endregion
    }
}