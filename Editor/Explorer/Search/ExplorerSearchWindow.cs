using System;
using K10.Common;
using Rogue.REditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    public class ExplorerSearchWindow : EditorWindow, IExplorerWindow
    {
        #region Drawing

        public Object ExplorerOwner => ExplorerEditorConfig.Instance;
        public ExplorerTabsPanel TabsPanel { get; private set; }
        public ExplorerInspectors Inspectors { get; private set; }
        public static bool TabsAreCollapsed { get; private set; }

        private TwoPaneSplitView mainSplitView;

        public void CreateGUI()
        {
            if (TabsPanel != null) return;

            ExplorerEditorLib.InstantiateSearchWindow(rootVisualElement);

            TabsPanel = rootVisualElement.Q<ExplorerTabsPanel>("explorer-tabsPanel");
            TabsPanel.SetWindow(this);
            Inspectors = rootVisualElement.Q<ExplorerInspectors>("explorer-inspectors");
            Inspectors.SetWindow(this);
            mainSplitView = rootVisualElement.Q<TwoPaneSplitView>("MainSplitView");

            SetupHotkeys();
        }

        public void Rebuild(bool changedProfile)
        {
            Instance = this;

            if (TabsPanel == null)
            {
                rootVisualElement.Clear();
                CreateGUI();
            }

            if (TabsAreCollapsed) mainSplitView.CollapseChild(0);

            TabsPanel!.style.display = DisplayStyle.Flex;
            Inspectors.style.display = DisplayStyle.Flex;

            TabsPanel.RebuildContent(true);
            Inspectors.Rebuild(changedProfile);

            SetupToolbar();
            UpdateToolbarButtons();
        }

        #endregion

        #region Toolbar

        private ToolbarButton favoritesButton;
        private ToolbarButton collapseButton;

        private void SetupToolbar()
        {
            if (favoritesButton != null) return;

            favoritesButton = rootVisualElement.Q<ToolbarButton>("FavoritesButton");
            favoritesButton.clicked += OpenFavoritesPicker;

            var refreshButton = rootVisualElement.Q<ToolbarButton>("RefreshButton");
            refreshButton.clicked += () => Rebuild(false);

            collapseButton = rootVisualElement.Q<ToolbarButton>("CollapseButton");
            collapseButton.clicked += ToggleCollapsed;

            var searchButton = rootVisualElement.Q<ToolbarButton>("SearchButton");
            searchButton.clicked += TryOpenSearch;
        }

        private void UpdateToolbarButtons()
        {
            var isCollapsed = TabsAreCollapsed;
            collapseButton.text = isCollapsed ? "→" : "←";
            favoritesButton.style.display = isCollapsed ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OpenFavoritesPicker()
        {
            var menu = new GenericMenu();
            var saveData = ((IExplorerWindow)this).ExplorerOwnerSaveData;

            foreach (var (target, propertyPath) in saveData.favorites)
            {
                var entryName = ExplorerEditorLib.GetDisplayName(target, propertyPath);
                var isOpened = Inspectors.IsOpened(target, propertyPath);

                menu.AddItem(new GUIContent(entryName), isOpened, () =>
                {
                    if (isOpened) Inspectors.CloseInspector(target, propertyPath);
                    else Inspectors.OpenInspector(target, propertyPath, null);

                    Rebuild(false);
                });
            }

            menu.ShowAsContext();
        }

        private void ToggleCollapsed()
        {
            TabsAreCollapsed = !TabsAreCollapsed;

            if (TabsAreCollapsed) mainSplitView.CollapseChild(0);
            else mainSplitView.UnCollapse();

            UpdateToolbarButtons();
        }

        #endregion

        #region Hotkeys

        private void SetupHotkeys()
        {
            rootVisualElement.focusable = true;
            rootVisualElement.Focus();

            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (EditorUtils.IsEditingData) return;
                if (evt.keyCode == KeyCode.F5) Rebuild(false);
                else if (evt.keyCode == KeyCode.F && evt.ctrlKey) TryOpenSearch();
            });
        }

        private void TryOpenSearch()
        {
            ClassTreePicker.Draw(EditorUtils.GetRectAtMouse(), typeof(ExplorerSearchConfigBase), null, OnTypeSelected);

            void OnTypeSelected(Type newSelection)
            {
                var tabId = Guid.NewGuid().ToString();
                var searchConfig = (ExplorerSearchConfigBase) Activator.CreateInstance(newSelection, tabId);
                var tabBuilder = new ExplorerSearchTabBuilder(this, searchConfig, true);
                TabsPanel.CreateOrSelectTab(tabId, "New Search", tabBuilder);
            }
        }

        #endregion

        #region Window Setup

        public static ExplorerSearchWindow Instance { get; private set; }

        private void Initialize()
        {
            try
            {
                if (this.IsNullOrDestroyed()) return;

                var isOpening = false;

                if (Instance != this)
                {
                    if (Instance == null)
                    {
                        Instance = this;
                        isOpening = true;
                    }
                    else return;
                }

                Rebuild(isOpening);
            }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private void OnEnable() => EditorUtils.RunDelayedOnce(Initialize);

        private void OnDisable()
        {
            TabsPanel?.WindowClosed();

            if (Instance == this)
                Instance = null;
        }

        [MenuItem("Rogue/Windows/Explorer Search %E")]
        public static void ShowWindow()
        {
            if (Instance != null) {
                Instance.Focus();
                return;
            }

            var instance = GetWindow<ExplorerSearchWindow>("Explorer Search");
            instance.Show();
        }

        #endregion
    }
}