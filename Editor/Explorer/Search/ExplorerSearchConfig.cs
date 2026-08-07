using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Rogue.RuntimeEditor;
using Skyx.RuntimeEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    public enum EExplorerSearchMode
    {
        Union,
        Intersect,
        MustIntersect,
    }

    [Serializable]
    public abstract class ExplorerSearchConfigBase
    {
        public readonly string tabID;
        public string name = "New Search";

        public int SourcesCount { get; protected set; }

        public abstract bool HasSources { get; }

        public bool HasResults => ResultsCount > 0;
        public abstract int ResultsCount { get; }
        public abstract int FiltersCount { get; }

        public abstract IEnumerable<Object> Results { get; }
        public abstract Dictionary<string, object> GetInternalResults(Object key);

        public abstract void RunFilters();
        public abstract UniTask FetchSources(CancellationToken token);

        protected ExplorerSearchConfigBase(string tabID) => this.tabID = tabID;
    }

    [Serializable]
    public abstract class ExplorerSearchConfig<T> : ExplorerSearchConfigBase where T : Object
    {
        [SerializeField, SerializeReference, Scoped(EScopePreset.FoldoutNameSummary)]
        private ExplorerSearchSourcesProvider<T> sourcesProvider;

        [SerializeField, SerializeReference]
        private List<IExplorerSearchFilter<T>> filters = new();

        [SerializeField] private EExplorerSearchMode searchMode = EExplorerSearchMode.Union;

        [SerializeField, SerializeReference, Scoped(EScopePreset.FoldoutNameSummary), SerializedRefOptions("No Batch Execute")]
        private ExplorerBatchExecute<T> batchExecute;

        public IEnumerable<T> Sources { get; private set; }

        public Dictionary<T, Dictionary<string, object>> TResults { get; private set; } = new();

        public override IEnumerable<Object> Results => TResults.Keys;
        public override Dictionary<string, object> GetInternalResults(Object key) => TResults[(T)key];

        public override bool HasSources => Sources != null && SourcesCount > 0;
        public override int FiltersCount => filters.Count;
        public override int ResultsCount => TResults?.Count ?? 0;

        private int lastProviderHash;

        public override void RunFilters()
        {
            if (TResults == null) TResults = new Dictionary<T, Dictionary<string, object>>();
            else TResults.Clear();

            if (!HasSources) return;

            var filterProperties = new Dictionary<string, object>();

            foreach (var candidate in Sources)
            {
                var selectedProperties = new Dictionary<string, object>();
                var valid = true;
                var isFirst = true;

                foreach (var filter in filters)
                {
                    if (filter == null) continue;

                    filterProperties.Clear();
                    if (!filter.FitsFilter(candidate, filterProperties))
                    {
                        valid = false;
                        break;
                    }

                    if (isFirst)
                    {
                        MergeDictionaries(filterProperties, selectedProperties);
                        isFirst = false;

                        if (searchMode is EExplorerSearchMode.MustIntersect && selectedProperties.Count == 0)
                        {
                            valid = false;
                            break;
                        }

                        continue;
                    }

                    if (searchMode is EExplorerSearchMode.Union)
                        MergeDictionaries(filterProperties, selectedProperties);
                    else
                    {
                        IntersectDictionaries(filterProperties, selectedProperties);

                        if (searchMode is EExplorerSearchMode.MustIntersect && selectedProperties.Count == 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid) TResults.Add(candidate, selectedProperties);
            }
        }

        private void MergeDictionaries(Dictionary<string, object> source, Dictionary<string, object> target)
        {
            foreach (var (key, value) in source)
                target[key] = value;
        }

        private void IntersectDictionaries(Dictionary<string, object> source, Dictionary<string, object> target)
        {
            foreach (var key in target.Keys.ToList())
            {
                if (source.ContainsKey(key)) continue;
                target.Remove(key);
            }
        }

        public override async UniTask FetchSources(CancellationToken token)
        {
            if (sourcesProvider == null)
            {
                SourcesCount = 0;
                Sources = null;

                return;
            }

            var providerHash = sourcesProvider.GetFetchHash();
            if (Sources != null && SourcesCount > 0 && providerHash == lastProviderHash) return;
            lastProviderHash = providerHash;

            Sources = await sourcesProvider.Fetch(token);
            SourcesCount = Sources?.Count() ?? 0;
        }

        protected ExplorerSearchConfig(string tabID) : base(tabID) {}

        protected ExplorerSearchConfig(string tabID, ExplorerSearchSourcesProvider<T> sourcesProvider) : base(tabID)
        {
            this.sourcesProvider = sourcesProvider;
        }
    }
}