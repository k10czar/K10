using System;
using System.Threading;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    public class ExplorerSearchTabBuilder : IExplorerTabBuilder
    {
        private readonly ExplorerSearchConfigBase searchConfig;
        private readonly IExplorerWindow window;

        private SerializedProperty searchProperty;
        private CancellationTokenSource cancellationToken;
        private bool searching;
        private Foldout resultsFoldout;
        private VisualElement resultsHolder;

        public void Rebuild(IExplorerWindow _, VisualElement root)
        {
            PropertyCollection.RegisterChanged(ExplorerEditorConfig.Instance.GetInstanceID(), ReRunFilters);

            var drawerHolder = new VisualElement()  { name = "search-property-drawer" };
            root.Add(drawerHolder);
            var drawer = new IMGUIContainer(DrawInspector);
            drawerHolder.Add(drawer);

            var newElement = ExplorerEditorLib.InstantiateSearchResults();
            resultsFoldout = newElement.Q<Foldout>("ExplorerSearchResults");
            root.Add(resultsFoldout);

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

        private void ReRunFilters()
        {
            try
            {
                if (searchProperty == null) ResetSerializedProperty();

                searching = true;
                cancellationToken = new CancellationTokenSource();

                resultsHolder.Clear();

                if (!searchConfig.HasSources) searchConfig.FetchSources(cancellationToken.Token);
                searchConfig.RunFilters();

                foreach (var result in searchConfig.Results)
                {
                    var newEntry = ExplorerEntryView.Create(window, result, string.Empty, null);
                    resultsHolder.Add(newEntry);
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

        private void ResetSerializedProperty()
        {
            var (saveIndex, saveData) = ExplorerEditorConfig.GetExplorerSaveInfo(window.ExplorerOwner);
            var index = saveData.searches.FindIndex(search => search == searchConfig);
            if (index == -1)
            {
                Debug.LogError("Could not find search config!");
                searchProperty = null;
                return;
            }

            ExplorerEditorConfig.SerializedObjInstance.Update();
            searchProperty = ExplorerEditorConfig.SerializedObjInstance.FindProperty($"explorerSaves.Array.data[{saveIndex}].searches.Array.data[{index}]");
        }

        public void TabClosed()
        {
            var config = ExplorerEditorConfig.Instance;

            Undo.RecordObject(config, "Remove favorite");
            window.ExplorerOwnerSaveData.searches.Remove(searchConfig);
            PropertyCollection.ApplyDirectChanges(config);

            searchProperty = null;

            WindowClosed();
        }

        public void Minimized() => WindowClosed();

        public void WindowClosed()
        {
            cancellationToken?.Cancel();
            cancellationToken?.Dispose();
            cancellationToken = null;

            PropertyCollection.RegisterChanged(ExplorerEditorConfig.Instance.GetInstanceID(), ReRunFilters);
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