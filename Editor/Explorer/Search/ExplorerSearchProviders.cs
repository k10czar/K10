using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using K10.Common;
using Rogue.Helpers;
using Rogue.REditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    public abstract class ExplorerSearchSourcesProvider<T> : IContentEditorInfo where T : Object
    {
        public abstract string ContentName { get; }
        public abstract UniTask<IEnumerable<T>> Fetch(CancellationToken token);

        protected virtual IEnumerator<object> GetEqualityComponents() { yield break; }

        public int GetFetchHash()
        {
            var list = new List<object>() { GetType().GetHashCode() };

            var components = GetEqualityComponents();
            while (components.MoveNext())
                list.Add(components.Current);

            return NumbersLib.GenerateStableHash(list);
        }
    }

    [Serializable]
    public class TargetExplorerSourcesProvider<T> : ExplorerSearchSourcesProvider<T> where T : Object
    {
        public override string ContentName => $"Search {sources.ToInspectorName("targets")}";

        [SerializeField] private List<T> sources = new();

        public override UniTask<IEnumerable<T>> Fetch(CancellationToken token) => UniTask.FromResult<IEnumerable<T>>(sources.ToList());

        protected override IEnumerator<object> GetEqualityComponents()
        {
            foreach (var source in sources)
                yield return source.GetInstanceID();
        }
    }

    [Serializable]
    public class AssetDatabaseExplorerSourcesProvider<T> : ExplorerSearchSourcesProvider<T> where T : ScriptableObject
    {
        public override string ContentName => $"Search all from AssetDatabase";

        public override UniTask<IEnumerable<T>> Fetch(CancellationToken token) => UniTask.FromResult<IEnumerable<T>>(AssetDatabaseUtils.GetAll<T>());
    }

    [Serializable]
    public class UnitySearchExplorerSourcesProvider<T> : ExplorerSearchSourcesProvider<T> where T : Object
    {
        public override string ContentName => $"Search {(searchScene ? "Scene" : "Project")} using UnitySearch";

        [SerializeField, BoolOptions("Where to Search", "Search Scene", "Search Project")]
        private bool searchScene;

        public override async UniTask<IEnumerable<T>> Fetch(CancellationToken token)
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

        protected override IEnumerator<object> GetEqualityComponents() { yield return searchScene; }
    }
}