using System.Collections.Generic;
using System.Linq;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    public static class ExplorerEditorLib
    {
        #region UXML Tree Assets

        private const string FolderPath = "Assets/Plugins/K10/Core/Editor/Explorer/UXML";

        private static VisualTreeAsset explorerEntryTreeAsset;
        public static VisualElement InstantiateExplorerEntry()
        {
            if (explorerEntryTreeAsset == null)
                explorerEntryTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{FolderPath}/ExplorerEntryView.uxml");

            var newEntry = new VisualElement();
            explorerEntryTreeAsset.CloneTree(newEntry);

            return newEntry;
        }

        private static VisualTreeAsset inspectorToolbarTreeAsset;
        public static VisualElement InstantiateInspectorToolbar()
        {
            if (inspectorToolbarTreeAsset == null)
                inspectorToolbarTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{FolderPath}/ExplorerInspectorToolbar.uxml");

            var newEntry = new VisualElement();
            inspectorToolbarTreeAsset.CloneTree(newEntry);

            return newEntry;
        }

        private static VisualTreeAsset searchTabTreeAsset;
        public static VisualElement InstantiateSearchTab()
        {
            if (searchTabTreeAsset == null)
                searchTabTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{FolderPath}/ExplorerSearchTab.uxml");

            var newEntry = new VisualElement();
            searchTabTreeAsset.CloneTree(newEntry);

            return newEntry;
        }

        private static VisualTreeAsset searchWindowTreeAsset;
        public static void InstantiateSearchWindow(VisualElement root)
        {
            if (searchWindowTreeAsset == null)
                searchWindowTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{FolderPath}/ExplorerSearchEditorWindow.uxml");

            searchWindowTreeAsset.CloneTree(root);
        }

        #endregion

        public static string GetDisplayName(Object target, string propertyPath)
        {
            if (target == null) return "Missing Reference!";

            if (!string.IsNullOrEmpty(propertyPath))
            {
                var serializedObj = PropertyCollection.GetSerializedObject(target);
                var property = serializedObj.FindProperty(propertyPath);

                if (property.GetValue() is IContentEditorInfo propertyInfo)
                    return propertyInfo.ContentName;

                return $"{target.name}:{propertyPath}";
            }

            if (target is IContentEditorInfo contentInfo && !string.IsNullOrEmpty(contentInfo.ContentName))
                return contentInfo.ContentName;

            return target.name;
        }

        public static ExplorerEntryDef CreateExplorerEntry(ExplorerInspectorInfo info, bool isInternal)
            => CreateExplorerEntry(info.window, info.target, info.propertyPath, info.breadcrumbs, isInternal);

        public static ExplorerEntryDef CreateExplorerEntry(IExplorerWindow window, Object target, string propertyPath, List<Object> breadcrumbs, bool isInternal)
        {
            var inspectors = window.Inspectors;
            var saveData = window.ExplorerOwnerSaveData;

            var isFavorite = saveData.IsFavorite(target, propertyPath);
            var isPinned = saveData.IsPinned(target, propertyPath);
            var isOpened = inspectors.IsOpened(target, propertyPath);

            var entryName = GetDisplayName(target, propertyPath);
            var (mainTooltip, mainColor) = GetOpenedStyle(isOpened);
            var mainButton = new ExplorerButtonDef(entryName, ToggleInspectorOpen, mainTooltip, mainColor);

            var (favTooltip, favColor) = GetFavoriteStyle(isFavorite);
            var favoriteButton = new ExplorerButtonDef("★", ToggleFavorite, favTooltip, favColor);

            var (pinTooltip, pinColor) = GetPinnedStyle(isPinned);
            var pinButton = new ExplorerButtonDef("📍︎", TogglePinned, pinTooltip, pinColor);

            var closeButton = isInternal ? new ExplorerButtonDef("✕", ToggleInspectorOpen, "Close inspector", EColor.Danger) { isDisabled = !isOpened } : null;

            var infoColor = SkopedDrawer.isShowingDescriptions ? EColor.Info : EColor.Clear;
            var infoButton = isInternal ? new ExplorerButtonDef("?", ToggleDescriptions, "Show descriptions and extra info on genOperators", infoColor) : null;

            ExplorerButtonDef[] buttons;

            if (string.IsNullOrEmpty(propertyPath))
            {
                var openDetachedButton = new ExplorerButtonDef("↔", OpenDetachedInspector, "Open in a separate inspector");
                buttons = isInternal
                    ? new[] { closeButton, pinButton, favoriteButton, openDetachedButton, infoButton }
                    : new[] { pinButton, favoriteButton, openDetachedButton };
            }
            else
            {
                buttons = isInternal
                    ? new[] { closeButton, pinButton, favoriteButton, infoButton }
                    : new[] { pinButton, favoriteButton };
            }

            return new ExplorerEntryDef(window, target, propertyPath, breadcrumbs?.ToList(), mainButton, buttons);
        }

        #region Callbacks

        public static void OpenOrHighlightInspector(IExplorerWindow window, Object target, string propertyPath, List<Object> breadcrumbs)
        {
            var inspectors = window.Inspectors;
            var isOpened = inspectors.IsOpened(target, propertyPath);

            if (isOpened) inspectors.HighlightInspector(target, propertyPath);
            else
            {
                inspectors.OpenInspector(target, propertyPath, breadcrumbs);
                window.Rebuild(false);
            }
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

            entry.window.Rebuild(false);
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

        private static void ToggleDescriptions(ExplorerEntryDef entry)
        {
            SkopedDrawer.isShowingDescriptions = !SkopedDrawer.isShowingDescriptions;
            entry.window.Rebuild(false);
        }

        #endregion

        #region Button Styles

        private static (string, EColor) GetOpenedStyle(bool isOpened) => isOpened ? ("Open this editor", EColor.Success) : ("Close this editor", EColor.Clear);
        private static (string, EColor) GetFavoriteStyle(bool isFavorite) => isFavorite ? ("Remove from favorites", EColor.Warning) : ("Add to favorites", EColor.Clear);
        private static (string, EColor) GetPinnedStyle(bool isPinned) => isPinned ? ("Pin this editor", EColor.Success) : ("Unpin this editor", EColor.Clear);

        #endregion
    }
}