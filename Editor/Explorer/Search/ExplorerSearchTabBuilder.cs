using System;
using System.Collections.Generic;
using System.Threading;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    public class ExplorerSearchTabBuilder : IExplorerTabBuilder
    {
        private readonly IExplorerWindow window;
        private ExplorerSearchConfigBase searchConfig;

        private SerializedProperty searchProperty;
        private CancellationTokenSource cancellationToken;
        private bool searching;
        private Foldout resultsFoldout;
        private VisualElement resultsHolder;

        public void Rebuild(IExplorerWindow _, VisualElement root)
        {
            PropertyCollection.RegisterChanged(ExplorerEditorConfig.Instance.GetInstanceID(), ReRunFilters);

            var newElement = ExplorerEditorLib.InstantiateSearchTab();
            root.Add(newElement);

            var configFoldout = newElement.Q<Foldout>("ExplorerSearchConfigs");
            var drawerHolder = configFoldout.Q<VisualElement>("unity-content");
            drawerHolder.AddToClassList("search-property-drawer");

            var drawer = new IMGUIContainer(DrawInspector);
            drawerHolder.Add(drawer);

            resultsFoldout = newElement.Q<Foldout>("ExplorerSearchResults");
            resultsHolder = resultsFoldout.Q<VisualElement>("unity-content");

            ReRunFilters();
        }

        private void DrawInspector()
        {
            if (searchProperty == null)
            {
                EditorGUILayout.HelpBox("Missing searchProperty!", MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(searchProperty, GUIContent.none, true);

            if (searching) EditorGUILayout.HelpBox("Refreshing Search!", MessageType.Info);
        }

        private void ReleasePropertyHighlights()
        {
            if (searchConfig == null) return;
            if (!searchConfig.HasResults) return;

            foreach (var result in searchConfig!.Results)
            {
                var innerProps = searchConfig.GetInternalResults(result);

                foreach (var (owner, path) in innerProps.Keys)
                    EditorPropertyHighlights.Release((owner.GetInstanceID(), path));
            }
        }

        private async void ReRunFilters()
        {
            try
            {
                ReleasePropertyHighlights();

                if (searchProperty == null && !ResetSerializedProperty()) return;

                searching = true;
                cancellationToken = new CancellationTokenSource();

                resultsHolder.Clear();

                await searchConfig.FetchSources(cancellationToken.Token);
                searchConfig.RunFilters();

                ExplorerEntryView.isOddEntry = false;

                foreach (var result in searchConfig.Results)
                {
                    var newEntry = ExplorerEntryView.Create(window, result, string.Empty, null);
                    resultsHolder.Add(newEntry);

                    var innerProps = searchConfig.GetInternalResults(result);
                    var breadcrumbs = new List<Object> { result };

                    foreach (var (owner, path) in innerProps.Keys)
                    {
                        EditorPropertyHighlights.Add((owner.GetInstanceID(), path));
                        var innerEntry = ExplorerEntryView.Create(window, result, path, breadcrumbs);
                        newEntry.AddInternalContent(innerEntry);
                    }
                }

                resultsFoldout.text = $"Results {searchConfig.ResultsCount}/{searchConfig.SourcesCount}";

                searching = false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to perform search {searchConfig.name}");
                Debug.LogException(exception);

                searching = false;
            }
            finally
            {
                cancellationToken?.Dispose();
                cancellationToken?.Dispose();
                cancellationToken = null;
            }
        }

        private bool ResetSerializedProperty()
        {
            var (saveIndex, saveData) = ExplorerEditorConfig.GetExplorerSaveInfo(window.ExplorerOwner);
            var index = saveData.searches.FindIndex(search => search == searchConfig);
            if (index == -1)
            {
                searchProperty = null;
                return false;
            }

            ExplorerEditorConfig.SerializedObjInstance.Update();
            searchProperty = ExplorerEditorConfig.SerializedObjInstance.FindProperty($"explorerSaves.Array.data[{saveIndex}].searches.Array.data[{index}]");

            return true;
        }

        public void TabClosed()
        {
            WindowClosed();

            var config = ExplorerEditorConfig.Instance;

            Undo.RecordObject(config, "Remove favorite");
            window.ExplorerOwnerSaveData.searches.Remove(searchConfig);
            PropertyCollection.ApplyDirectChanges(config);

            searchConfig = null;
            searchProperty = null;
        }

        public void Minimized() => WindowClosed();

        public void WindowClosed()
        {
            ReleasePropertyHighlights();

            cancellationToken?.Cancel();
            cancellationToken?.Dispose();
            cancellationToken = null;

            PropertyCollection.DeregisterChanged(ExplorerEditorConfig.Instance.GetInstanceID(), ReRunFilters);
        }

        public ExplorerSearchTabBuilder(IExplorerWindow window, ExplorerSearchConfigBase searchConfig, bool isNewConfig)
        {
            this.window = window;
            this.searchConfig = searchConfig;

            if (isNewConfig)
            {
                var config = ExplorerEditorConfig.Instance;

                Undo.RecordObject(config, "Add search");
                window.ExplorerOwnerSaveData.searches.Add(searchConfig);
                PropertyCollection.ApplyDirectChanges(config);
            }

            ResetSerializedProperty();
        }
    }
}