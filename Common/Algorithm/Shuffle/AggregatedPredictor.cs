using System.Collections.Generic;
using System;
using UnityEngine;

public class AggregatedPredictor<T>
{
    public List<SubsetSelectorPredictor<T>> nestedCrawlers = new();

    public RangeSummary Score = new();
    public RangeSummary ElementsCount = new();

    bool _isTooExpensive = false;
    public bool IsTooExpensive => _isTooExpensive;

    double _elapsedSeconds = 0;
    public double ElapsedSeconds => _elapsedSeconds;

    ulong _variationsCount = 0;
    public ulong VariationsCount => _variationsCount;
    ulong _variationsWithPermutationCount = 0;
    public ulong VariationsWithPermutationCount => _variationsWithPermutationCount;

    Dictionary<T,(double min, double avg, double max)> _elementRange = new();

    public IEnumerator<KeyValuePair<T,(double min, double avg, double max)>> GetElementAveragesEnumerator() => _elementRange.GetEnumerator();

    public void Calculate( IAggregatedSubsetSelector<T> agg, Func<T,double> Scorer = null )
    {
        nestedCrawlers.Clear();

        Score.SetZero();
        ElementsCount.SetZero();
        _isTooExpensive = false;

        _elapsedSeconds = 0;

        if( agg == null ) return;

        _variationsCount = 1;
        _variationsWithPermutationCount = 1;

        for ( int i = 0; i < agg.Count && !_isTooExpensive; i++ )
        {
            var crawler = new SubsetSelectorPredictor<T>();
            crawler.SetScores( Scorer );
            crawler.Calculate( agg.GetEntry(i) );
            _elapsedSeconds += crawler.TimeLimit.ElapsedSeconds();
            nestedCrawlers.Add( crawler );

            Score.Combine( crawler.Score );
            ElementsCount.Combine( crawler.ElementsCount );

            _isTooExpensive |= crawler.TimeLimit.IsExausted;

            _variationsCount *= crawler.VariationsCount;
            _variationsWithPermutationCount *= crawler.VariationsWithPermutationCount;

            var enumerator = crawler.GetElementAveragesEnumerator();
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
    public string Stringfy( string score = "Score", string count = "Elements Count", string separator = " " ) => _isTooExpensive ? "Time limit exeeded" : $"{score}: {Score}{separator}{count}: {ElementsCount}{separator}Variations(With Permutations): {_variationsCount}({_variationsWithPermutationCount})";
    public override string ToString() => Stringfy();
}
