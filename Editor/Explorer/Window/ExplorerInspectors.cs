using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rogue.Helpers;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    [UxmlElement]
    public partial class ExplorerInspectors : VisualElement
    {
        public IExplorerWindow Window { get; private set; }

        #region Interface

        private readonly List<ExplorerInspectorInfo> openedInspectors = new();

        public bool HasOpenedInspectors => openedInspectors.Count > 0;

        public ExplorerInspectorInfo GetInspectorInfo(Object target, string propertyPath)
            => openedInspectors.FirstOrDefault(entry => entry.target == target && entry.propertyPath == propertyPath);

        public bool IsOpened(Object target, string propertyPath)
            => GetInspectorInfo(target, propertyPath) != null;

        public void OpenInspector(Object target, string propertyPath, List<Object> breadcrumbs, bool isPinned = false)
        {
            var info = new ExplorerInspectorInfo(Window, target, propertyPath, breadcrumbs, isPinned);

            if (HasOpenedInspectors)
            {
                var lastInspector = openedInspectors[0];
                if (!lastInspector.isPinned) openedInspectors.RemoveAt(0);
            }

            openedInspectors.Insert(0, info);
        }

        public void CloseInspector(Object target, string propertyPath)
        {
            var inspector = GetInspectorInfo(target, propertyPath);

            if (inspector.isPinned) RemovePin(target, propertyPath);
            else openedInspectors.Remove(inspector);
        }

        public void AddToFavorites(Object target, string propertyPath)
        {
            var config = ExplorerEditorConfig.Instance;
            var saveEntry = Window.ExplorerOwnerSaveData;

            Undo.RecordObject(config, "Add favorite");
            saveEntry.AddFavorite(target, propertyPath);
            PropertyCollection.ApplyDirectChanges(config);
        }

        public void RemoveFromFavorites(Object target, string propertyPath)
        {
            var config = ExplorerEditorConfig.Instance;
            var saveEntry = Window.ExplorerOwnerSaveData;

            Undo.RecordObject(config, "Remove favorite");
            saveEntry.RemoveFavorite(target, propertyPath);
            PropertyCollection.ApplyDirectChanges(config);
        }

        public void AddPin(Object target, string propertyPath, List<Object> entryBreadcrumbs)
        {
            var config = ExplorerEditorConfig.Instance;
            var saveEntry = Window.ExplorerOwnerSaveData;

            Undo.RecordObject(config, "Add pin");
            saveEntry.Pin(target, propertyPath);
            PropertyCollection.ApplyDirectChanges(config);

            var targetInspector = GetInspectorInfo(target, propertyPath);
            if (targetInspector == null) OpenInspector(target, propertyPath, entryBreadcrumbs, true);
            else targetInspector.isPinned = true;
        }

        public void RemovePin(Object target, string propertyPath)
        {
            var config = ExplorerEditorConfig.Instance;
            var saveEntry = Window.ExplorerOwnerSaveData;

            Undo.RecordObject(config, "Remove pin");
            saveEntry.RemovePin(target, propertyPath);
            PropertyCollection.ApplyDirectChanges(config);

            var targetInspector = GetInspectorInfo(target, propertyPath);
            if (targetInspector == null) return;

            openedInspectors.Remove(targetInspector);
        }

        private void OpenAllPinned()
        {
            var saveEntry = Window.ExplorerOwnerSaveData;

            foreach (var (target, path) in saveEntry.pinned)
                OpenInspector(target, path, null, true);
        }

        public void HighlightInspector(Object target, string propertyPath)
        {
            var index = openedInspectors.FindIndex(entry => entry.target == target && entry.propertyPath == propertyPath);
            if (index == -1) return;

            var (toolbar, _) = inspectorsHolder[index];
            toolbar.AddToClassList("highlighted");
            schedule.Execute(() => toolbar.RemoveFromClassList("highlighted")).ExecuteLater(5000);
        }

        #endregion

        #region Draw

        private readonly List<TwoPaneSplitView> splits = new();
        private readonly List<(ExplorerInspectorToolbar, VisualElement)> inspectorsHolder = new();

        private VisualElement inspectorsRoot;

        public void SetWindow(IExplorerWindow windowRef) => Window = windowRef;

        public void Rebuild(bool changedOwner)
        {
            inspectorsRoot = this.Q<VisualElement>("inspectors-holder");

            if (changedOwner)
            {
                ClearAll();
                OpenAllPinned();
            }

            OpenEnoughInspectorHolders();

            for (var index = 0; index < inspectorsHolder.Count; index++)
            {
                var info = openedInspectors[index];
                var (toolbar, holder) = inspectorsHolder[index];

                holder.Clear();

                var drawer = new IMGUIContainer(() => DrawInspector(info));
                holder.Add(drawer);

                toolbar.Setup(Window, info);
            }

            if (openedInspectors.Count > 1)
                EditorUtils.RunDelayedOnce(ResetAllPaneSizes);
        }

        private static void DrawInspector(ExplorerInspectorInfo info)
        {
            if (info.objectEditor != null)
                info.objectEditor.OnInspectorGUI();
            else
            {
                if (SkyxLayout.DrawTitle(info.target))
                    EditorGUIUtility.PingObject(info.target);

                EditorGUILayout.PropertyField(info.property, GUIContent.none, true);
            }
        }

        private void OpenEnoughInspectorHolders()
        {
            var openCount = openedInspectors.Count;

            if (openCount == inspectorsHolder.Count) return;
            if (openCount == 0)
            {
                ClearAll();
                return;
            }

            if (openCount > inspectorsHolder.Count)
            {
                if (inspectorsHolder.Count > 0) inspectorsHolder.ExtractLast();

                while (openCount > inspectorsHolder.Count)
                {
                    if (openCount > 1)
                        SplitView(openCount == inspectorsHolder.Count + 2);
                    else SetupInspectorHolder(inspectorsRoot);
                }
            }

            if (openCount < inspectorsHolder.Count)
            {
                var firstRemoval = true;
                while (openCount < inspectorsHolder.Count)
                {
                    var lastSplit = splits.ExtractLast();
                    lastSplit.RemoveFromHierarchy();

                    if (firstRemoval) inspectorsHolder.RemoveRange(inspectorsHolder.Count - 2, 2);
                    else inspectorsHolder.ExtractLast();

                    firstRemoval = false;
                }

                if (splits.Count == 0)
                {
                    inspectorsHolder.Clear();
                    SetupInspectorHolder(inspectorsRoot);
                }
                else
                {
                    var lastSplit = splits[^1];
                    var rightPanel = lastSplit.Q<VisualElement>("right-panel");
                    rightPanel.Clear();
                    SetupInspectorHolder(rightPanel);
                }
            }
        }

        private void ResetAllPaneSizes()
        {
            var targetSize = inspectorsRoot.resolvedStyle.width / inspectorsHolder.Count;

            var setDragLineMethod = typeof(TwoPaneSplitView).GetMethod("SetDragLineOffset", BindingFlags.NonPublic | BindingFlags.Instance);
            var setLineParams = new object[] { targetSize };

            for (var index = 0; index < splits.Count; index++)
            {
                var split = splits[index];

                if (index == splits.Count - 1)
                    split.Q<VisualElement>("right-panel").style.width = targetSize;

                split.Q<VisualElement>("left-panel").style.width = targetSize;
                split.fixedPaneInitialDimension = targetSize;

                setDragLineMethod!.Invoke(split, setLineParams);
            }
        }

        private void SplitView(bool createRightInspector)
        {
            var root = splits.Count > 0 ? splits[^1].Q<VisualElement>("right-panel") : inspectorsRoot;
            root.Clear();

            var split = new TwoPaneSplitView
            {
                fixedPaneIndex = 0,
                orientation = TwoPaneSplitViewOrientation.Horizontal
            };

            splits.Add(split);
            root.Add(split);

            var splitContent = split.Q<VisualElement>("unity-content-container");

            var leftPanel = new VisualElement { name = "left-panel" };
            splitContent.Add(leftPanel);

            var rightPanel = new VisualElement{ name = "right-panel" };
            splitContent.Add(rightPanel);

            SetupInspectorHolder(leftPanel);
            if (createRightInspector) SetupInspectorHolder(rightPanel);
        }

        private void SetupInspectorHolder(VisualElement root)
        {
            var holder = ExplorerEditorLib.InstantiateInspectorToolbar();
            holder.name = "inspector-holder";
            root.Add(holder);

            var scroll = new ScrollView();
            holder.Add(scroll);

            var toolbar = holder.Q<ExplorerInspectorToolbar>();
            var content = scroll.Q<VisualElement>("unity-content-container");

            inspectorsHolder.Add((toolbar, content));
        }

        private void ClearAll()
        {
            openedInspectors.Clear();
            inspectorsHolder.Clear();
            splits.Clear();
            inspectorsRoot.Clear();
        }

        #endregion
    }
}