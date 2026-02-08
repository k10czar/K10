using System.Collections.Generic;
using System;
using UnityEngine;

public class CompoundAggregatedPredictor<T,K> : BaseAggregatedPredictor<T> where T : IAggregatedSubsetSelector<K>
{
    public List<CompoundSubsetSelectorPredictor<T,K>> nestedCrawlers = new();
    Func<K, double> _scorer;

    public RangeSummary SubElementsCount = new();

    ulong _subElementsVariationsCount = 0;
    ulong _subElementsVariationsWithPermutationCount = 0;
    public ulong SubElementsVariationsCount => _subElementsVariationsCount;
    public ulong SubElementsVariationsWithPermutationCount => _subElementsVariationsWithPermutationCount;

    Dictionary<K,RangeSummary> _subElementRange = new();
    public IEnumerator<KeyValuePair<K,RangeSummary>> GetSubElementAveragesEnumerator() => _subElementRange.GetEnumerator();

    public void SetScorer(Func<K, double> scorer)
    {
        _scorer = scorer;
    }
    
    public string FullStringfy( string score = "Score", string count = "Elements Count", string subCount = "Sub Elements Count", string variations = "Variations", string subvariations = "SubVariations", string withPermutations = "With Permutations", string separator = " " ) 
            => 
                false ? 
                "Time limit exeeded" : 
                $"{score}: {Score}{separator}{count}: {ElementsCount}{separator}{subCount}: {SubElementsCount}{separator}{variations}({withPermutations}): {VariationsCount:n0}({VariationsWithPermutationCount:n0}){separator}{subvariations}({withPermutations}): {SubElementsVariationsCount:n0}({SubElementsVariationsWithPermutationCount:n0})";

    public override int SubPredictorCount => nestedCrawlers?.Count ?? 0;
    public override BaseSubsetSelectorPredictor<T> GetSubPredictor(int index) => nestedCrawlers != null && nestedCrawlers.Count > index ? nestedCrawlers[index] : null;
    protected override void ClearCrawlers()
    {
        nestedCrawlers.Clear();
        SubElementsCount.SetZero();
        _subElementsVariationsCount = 1;
        _subElementsVariationsWithPermutationCount = 1;
        foreach( var range in _subElementRange ) range.Value.SetZero();
    }
    protected override BaseSubsetSelectorPredictor<T> BuildCrawler(ISubsetSelector<T> subsetSelector)
    {
        var crawler = new CompoundSubsetSelectorPredictor<T,K>();
        crawler.SetScorer( _scorer );
        crawler.Calculate( subsetSelector );
        nestedCrawlers.Add( crawler );
        SubElementsCount.Combine( crawler.SubElementsCountRange );
        _subElementsVariationsCount *= crawler.SubElementVariationsCount;
        _subElementsVariationsWithPermutationCount *= crawler.SubElementVariationsWithPermutationCount;
        
        var enumerator = crawler.GetSubElementsRangeEnumerator();
        while (enumerator.MoveNext())
        {
            var entry = enumerator.Current;
            var entryRange = entry.range;
            var element = entry.element;
            if( element == null ) continue;
            if( !_subElementRange.TryGetValue( element, out var range ) ) 
            {
                range = new();
                range.SetZero();
            }
            // if( entryRange.WrongSum ) Debug.Log( $"Addind wrong sum from {element} {entryRange.ToStringFull()}" );
            range.Combine( entryRange );
            _subElementRange[element] = range;
        }

        return crawler;
    }
}
