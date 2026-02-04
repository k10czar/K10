using System.Collections.Generic;
using System;

public class CompoundAggregatedPredictor<T,K> where T : IAggregatedSubsetSelector<K>
{
    public List<CompoundSubsetSelectorPredictor<T,K>> nestedCrawlers = new();

    public RangeSummary Score = new();
    public RangeSummary ElementsCount = new();
    public RangeSummary SubElementsCount = new();

    bool _isTooExpensive = false;
    public bool IsTooExpensive => _isTooExpensive;

    double _elapsedSeconds = 0;
    public double ElapsedSeconds => _elapsedSeconds;

    ulong _variationsCount = 0;
    ulong _variationsWithPermutationCount = 0;
    ulong _subElementsVariationsCount = 0;
    ulong _subElementsVariationsWithPermutationCount = 0;

    public ulong VariationsCount => _variationsCount;
    public ulong VariationsWithPermutationCount => _variationsWithPermutationCount;
    public ulong SubElementsVariationsCount => _subElementsVariationsCount;
    public ulong SubElementsVariationsWithPermutationCount => _subElementsVariationsWithPermutationCount;

    Dictionary<T,(double min, double avg, double max)> _elementRange = new();

    public IEnumerator<KeyValuePair<T,(double min, double avg, double max)>> GetElementAveragesEnumerator() => _elementRange.GetEnumerator();
    
    public string Stringfy( string score = "Score", string count = "Elements Count", string subCount = "Sub Elements Count", string variations = "Variations", string subvariations = "SubVariations", string withPermutations = "With Permutations", string separator = " " ) 
            => 
                false ? 
                "Time limit exeeded" : 
                $"{score}: {Score}{separator}{count}: {ElementsCount}{separator}{subCount}: {SubElementsCount}{separator}{variations}({withPermutations}): {VariationsCount:n0}({VariationsWithPermutationCount:n0}){separator}{subvariations}({withPermutations}): {SubElementsVariationsCount:n0}({SubElementsVariationsWithPermutationCount:n0})";

    public void Calculate( IAggregatedSubsetSelector<T> agg, Func<K,double> Scorer = null )
    {
        nestedCrawlers.Clear();

        Score.SetZero();
        ElementsCount.SetZero();
        SubElementsCount.SetZero();
        _isTooExpensive = false;

        _elapsedSeconds = 0;

        if( agg == null ) return;

        _variationsCount = 1;
        _variationsWithPermutationCount = 1;
        _subElementsVariationsCount = 1;
        _subElementsVariationsWithPermutationCount = 1;

        for ( int i = 0; i < agg.Count && !_isTooExpensive; i++ )
        {
            var crawler = new CompoundSubsetSelectorPredictor<T,K>();
            crawler.SetScorer( Scorer );
            crawler.Calculate( agg.GetEntry(i) );
            _elapsedSeconds += crawler.TimeLimit.ElapsedSeconds();
            nestedCrawlers.Add( crawler );

            Score.Combine( crawler.Score );
            ElementsCount.Combine( crawler.ElementsCount );
            SubElementsCount.Combine( crawler.SubElementsCountRange );

            _isTooExpensive |= crawler.TimeLimit.IsExausted;

            _variationsCount *= crawler.VariationsCount;
            _variationsWithPermutationCount *= crawler.VariationsWithPermutationCount;
            _subElementsVariationsCount *= crawler.SubElementVariationsCount;
            _subElementsVariationsWithPermutationCount *= crawler.SubElementVariationsWithPermutationCount;

            var enumerator = crawler.GetElementsRangeEnumerator();
            while (enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                var entryRange = entry.range;
                var element = entry.element;
                _elementRange.TryGetValue( element, out var range );
                range.min += entryRange.min;
                range.avg += entryRange.avg;
                range.max += entryRange.max;
                _elementRange[element] = range;
            }
        }
    }

    public string DebugTimes => $"⏳{ElapsedSeconds:N2}s";
    public override string ToString() => Stringfy();
}
