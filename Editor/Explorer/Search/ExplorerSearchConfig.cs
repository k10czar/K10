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
    [Serializable]
    public abstract class ExplorerSearchConfigBase
    {
        public readonly string tabID;
        public string name;

        public int SourcesCount { get; protected set; }

        public abstract bool HasSources { get; }
        public abstract int ResultsCount { get; }
        public abstract int FiltersCount { get; }

        public abstract IEnumerable<Object> Results { get; }

        public abstract void RunFilters();
        public abstract UniTask FetchSources(CancellationToken token);

        protected ExplorerSearchConfigBase(string tabID) => this.tabID = tabID;
    }

    [Serializable]
    public abstract class ExplorerSearchConfig<T> : ExplorerSearchConfigBase where T : Object
    {
        [SerializeField, SerializeReference, Scoped(EScopePreset.HeaderNameSummary, EElementSize.SingleLine)]
        private IExplorerSearchSourcesProvider<T> sourcesProvider;

        [SerializeField, SerializeReference]
        private List<IExplorerSearchFilter<T>> filters = new();

        [SerializeField, SerializeReference, Scoped(EScopePreset.HeaderNameSummary, EElementSize.SingleLine), SerializedRefOptions("No Batch Execute")]
        private ExplorerBatchExecute<T> batchExecute;

        public IEnumerable<T> Sources { get; private set; }
        public List<T> TResults { get; private set; } = new();
        public override IEnumerable<Object> Results => TResults;

        public override bool HasSources => Sources != null && SourcesCount > 0;
        public override int FiltersCount => filters.Count;
        public override int ResultsCount => TResults?.Count ?? 0;

        public override void RunFilters()
        {
            TResults.Clear();

            if (!HasSources) return;

            foreach (var candidate in Sources!)
            {
                var valid = true;

                foreach (var filter in filters)
                {
                    if (filter == null) continue;
                    if (filter.FitsFilter(candidate)) continue;

                    valid = false;
                    break;
                }

                if (valid) TResults.Add(candidate);
            }
        }

        public override async UniTask FetchSources(CancellationToken token)
        {
            SourcesCount = 0;
            Sources = await sourcesProvider.Fetch(token);
            SourcesCount = Sources?.Count() ?? 0;
        }

        protected ExplorerSearchConfig(string tabID) : base(tabID)
        {
            name = $"{typeof(T).Name} Search";
        }

        protected ExplorerSearchConfig(string tabID, IExplorerSearchSourcesProvider<T> sourcesProvider) : this(tabID)
        {
            this.sourcesProvider = sourcesProvider;
        }
    }
}