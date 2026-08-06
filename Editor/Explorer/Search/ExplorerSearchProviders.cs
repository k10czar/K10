using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using K10.Common;
using Rogue.REditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    public interface IExplorerSearchSourcesProvider<T> : IContentEditorInfo where T : Object
    {
        public UniTask<IEnumerable<T>> Fetch(CancellationToken token);
    }

    [Serializable]
    public class TargetExplorerSourcesProvider<T> : IExplorerSearchSourcesProvider<T> where T : Object
    {
        public string ContentName => $"Search {sources.ToInspectorName("targets")}";

        [SerializeField] private List<T> sources = new();

        public UniTask<IEnumerable<T>> Fetch(CancellationToken token) => UniTask.FromResult<IEnumerable<T>>(sources.ToList());
    }

    [Serializable]
    public class AssetDatabaseExplorerSourcesProvider<T> : IExplorerSearchSourcesProvider<T> where T : ScriptableObject
    {
        public string ContentName => $"Search all from AssetDatabase";

        public UniTask<IEnumerable<T>> Fetch(CancellationToken token) => UniTask.FromResult<IEnumerable<T>>(AssetDatabaseUtils.GetAll<T>());
    }

    [Serializable]
    public class UnitySearchExplorerSourcesProvider<T> : IExplorerSearchSourcesProvider<T> where T : Object
    {
        public string ContentName => $"Search {(searchScene ? "Scene" : "Project")} using UnitySearch";

        [SerializeField, BoolOptions("Where to Search", "Search Scene", "Search Project")]
        private bool searchScene;

        public async UniTask<IEnumerable<T>> Fetch(CancellationToken token)
        {
            var set = new HashSet<T>();

            var provider = searchScene ? "scene" : "asset";
            var type = typeof(T);

            var isMonoBehaviour = type.IsSubclassOf(typeof(MonoBehaviour));

            var query = searchScene || !isMonoBehaviour
                ? $"t:{type.Name}"
                : $"t:{type.Name} (prefab:base or prefab:variant)";

            using var context = SearchService.CreateContext(provider, query);
            using var items = SearchService.Request(context);

            await UniTask.WaitUntil(() => !items.pending, cancellationToken: token);

            foreach (var item in items)
            {
                try
                {
                    var reference = item?.ToObject<T>();
                    if (reference != null) set.Add(reference);
                }
                catch (Exception exception) { Debug.LogException(exception); }
            }

            return set.ToList();
        }
    }
}