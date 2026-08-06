using System.Collections.Generic;
using Rogue.REditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    [UxmlElement]
    public partial class ExplorerEntryView : VisualElement
    {
        public static bool isOddEntry;

        private Button foldout;
        private VisualElement foldoutOffset;

        private Button mainContent;
        private VisualElement expandableContent;
        private ExplorerEntryOptions options;

        private ExplorerEntryDef myData;
        private ExplorerButtonDef currentDef;

        private void SetRefs()
        {
            if (mainContent != null) return;

            foldout = this.Q<Button>("FoldoutButton");
            foldoutOffset = this.Q<VisualElement>("FoldoutOffset");
            mainContent = this.Q<Button>("EntryContent");
            expandableContent = this.Q<VisualElement>("ExpandableContent");
            options = this.Q<ExplorerEntryOptions>("EntryOptions");

            foldoutOffset.style.display = DisplayStyle.Flex;
            foldout.style.display = DisplayStyle.None;
            foldout.AddToClassList("expanded");

            foldout.clicked += OnClickedFoldout;
            mainContent.clicked += OnClickedMainContent;
        }

        private void OnClickedFoldout()
        {
            var isExpanded = foldout.ClassListContains("expanded");

            if (isExpanded)
            {
                foldout.RemoveFromClassList("expanded");
                expandableContent.style.display = DisplayStyle.None;
            }
            else
            {
                foldout.AddToClassList("expanded");
                expandableContent.style.display = DisplayStyle.Flex;
            }
        }

        private void SetContent(ExplorerEntryDef entryDef, int nestingDepth)
        {
            SetRefs();

            myData = entryDef;
            currentDef = entryDef.mainButton;
            currentDef.BindVisuals(mainContent, this, false);

            options.SetOptions(myData);

            AddToClassList($"explorer-entry-{nestingDepth}");
            if (isOddEntry) AddToClassList("odd-entry");
            isOddEntry = !isOddEntry;
        }

        public void AddInternalContent(VisualElement extraContent)
        {
            expandableContent.Add(extraContent);
            foldout.style.display = DisplayStyle.Flex;
            foldoutOffset.style.display = DisplayStyle.None;
        }

        private void OnClickedMainContent()
        {
            if (currentDef == null)
            {
                Debug.LogError("Missing button def!");
                return;
            }

            currentDef.onClick(myData);
        }

        #region Static Interface

        public static ExplorerEntryView Create(IExplorerWindow window, Object target, string propertyPath, List<Object> breadcrumbs)
        {
            var entryDef = ExplorerEditorLib.CreateExplorerEntry(window, target, propertyPath, breadcrumbs, false);
            var newEntry = ExplorerEditorLib.InstantiateExplorerEntry();

            var entryView = newEntry.Q<ExplorerEntryView>();
            entryView.SetContent(entryDef, (breadcrumbs?.Count ?? 0) + 1);

            return entryView;
        }

        private static void UpdateAll(ExplorerEntryDef entry)
        {
            var inspectors = entry.window.Inspectors;
            var saveData = entry.window.ExplorerOwnerSaveData;

            var isFavorite = saveData.IsFavorite(entry.target, entry.propertyPath);
            var isPinned = saveData.IsPinned(entry.target, entry.propertyPath);
            var isOpened = inspectors.IsOpened(entry.target, entry.propertyPath);

            var mainButton = entry.mainButton;
            (mainButton.tooltip, mainButton.color) = GetOpenedStyle(isOpened);
            mainButton.UpdateVisuals();

            var pinButton = entry.options[0];
            (pinButton.tooltip, pinButton.color) = GetPinnedStyle(isPinned);
            pinButton.UpdateVisuals();

            var favoriteButton = entry.options[1];
            (favoriteButton.tooltip, favoriteButton.color) = GetFavoriteStyle(isFavorite);
            favoriteButton.UpdateVisuals();
        }

        private static void ToggleInspectorOpen(ExplorerEntryDef entry)
        {
            var inspectors = entry.window.Inspectors;
            var isOpened = inspectors.IsOpened(entry.target, entry.propertyPath);

            if (isOpened) inspectors.CloseInspector(entry.target, entry.propertyPath);
            else inspectors.OpenInspector(entry.target, entry.propertyPath, entry.breadcrumbs);

            entry.window.Rebuild(false);
        }

        private static void ToggleFavorite(ExplorerEntryDef entry)
        {
            var inspectors = entry.window.Inspectors;
            var saveData = entry.window.ExplorerOwnerSaveData;
            var isFavorite = saveData.IsFavorite(entry.target, entry.propertyPath);

            if (isFavorite) inspectors.RemoveFromFavorites(entry.target, entry.propertyPath);
            else inspectors.AddToFavorites(entry.target, entry.propertyPath);

            entry.window.TabsPanel.RebuildFavorites();
            UpdateAll(entry);
        }

        private static void TogglePinned(ExplorerEntryDef entry)
        {
            var inspectors = entry.window.Inspectors;
            var saveData = entry.window.ExplorerOwnerSaveData;
            var isPinned = saveData.IsPinned(entry.target, entry.propertyPath);

            if (isPinned) inspectors.RemovePin(entry.target, entry.propertyPath);
            else inspectors.AddPin(entry.target, entry.propertyPath, entry.breadcrumbs);

            entry.window.Rebuild(false);
        }

        private static void OpenDetachedInspector(ExplorerEntryDef entry) => EditorUtils.OpenOrFocusInspectorOn(entry.target);

        #region Button Styles

        private static (string, EColor) GetOpenedStyle(bool isOpened) => isOpened ? ("Open this editor", EColor.Success) : ("Close this editor", EColor.Clear);
        private static (string, EColor) GetFavoriteStyle(bool isFavorite) => isFavorite ? ("Remove from favorites", EColor.Warning) : ("Add to favorites", EColor.Clear);
        private static (string, EColor) GetPinnedStyle(bool isPinned) => isPinned ? ("Pin this editor", EColor.Success) : ("Unpin this editor", EColor.Clear);

        #endregion

        #endregion
    }

    public class ExplorerEntryDef
    {
        public readonly IExplorerWindow window;
        public readonly Object target;
        public readonly string propertyPath;
        public readonly List<Object> breadcrumbs;

        public readonly ExplorerButtonDef mainButton;
        public readonly ExplorerButtonDef[] options;

        public ExplorerEntryDef(IExplorerWindow window, Object target, string propertyPath, List<Object> breadcrumbs, ExplorerButtonDef mainButton, ExplorerButtonDef[] options)
        {
            this.window = window;
            this.target = target;
            this.propertyPath = propertyPath;
            this.breadcrumbs = breadcrumbs;
            this.mainButton = mainButton;
            this.options = options;
        }
    }
}