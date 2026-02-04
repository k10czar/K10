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

    public double[,] _elementCountChance;
    public double[] _elementAvg;
    public double[] _countOfAnyChance;
    public int[] _maxElementsCountOf;
    public int[,] _elementIds;
    public string[] _namesCache;

    public int _maxElementCount;
    public int _elementsCount;

    Dictionary<T,int> _elementId = new();

    Dictionary<T,(double min, double avg, double max)> _elementRange = new();

    public IEnumerator<KeyValuePair<T,(double min, double avg, double max)>> GetElementAveragesEnumerator() => _elementRange.GetEnumerator();

    public SubsetSelectorPredictor<T> GetSubPredictor(int index) => nestedCrawlers != null && nestedCrawlers.Count > index ? nestedCrawlers[index] : null;

    public bool HasChanceOfAnyElementCount(int count)
    {
        return _countOfAnyChance != null && _countOfAnyChance[count] > float.Epsilon;
    }

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
            crawler.SetScorer( Scorer );
            crawler.Calculate( agg.GetEntry(i) );
            _elapsedSeconds += crawler.TimeLimit.ElapsedSeconds();
            nestedCrawlers.Add( crawler );

            Score.Combine( crawler.Score );
            ElementsCount.Combine( crawler.ElementsCount );

            _isTooExpensive |= crawler.TimeLimit.IsExausted;

            _variationsCount *= crawler.VariationsCount;
            _variationsWithPermutationCount *= crawler.VariationsWithPermutationCount;

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

        for ( int i = 0; i < nestedCrawlers.Count; i++ )
        {
            var crawler = nestedCrawlers[i];
            var count = crawler._elements.Length;
            for ( int j = 0; j < count; j++ )
            {
                var element = crawler._elements[j];
                if( element == null ) continue;
                if( _elementId.TryGetValue( element, out var id ) ) continue;
                id = _elementId.Count;
                _elementId[element] = id;
            }
        }

        _elementsCount = _elementId.Count;
        if( _namesCache == null || _namesCache.Length != _elementsCount ) _namesCache = new string[_elementsCount];
        if( _maxElementsCountOf == null || _maxElementsCountOf.Length != _elementsCount ) _maxElementsCountOf = new int[_elementsCount];
        if( _elementAvg == null || _elementAvg.Length != _elementsCount ) _elementAvg = new double[_elementsCount];
        for( int i = 0; i < _elementsCount; i++ ) 
        {
            _maxElementsCountOf[i] = 0;
            _elementAvg[i] = 0;
        }

        _elementIds = new int[_elementsCount,nestedCrawlers.Count];

        for ( int i = 0; i < nestedCrawlers.Count; i++ )
        {
            for ( int j = 0; j < _elementsCount; j++ )
            {
                _elementIds[j,i] = -1;
            }

            var crawler = nestedCrawlers[i];
            var count = crawler._elements.Length;
            for ( int j = 0; j < count; j++ )
            {
                var element = crawler._elements[j];
                if( element == null ) continue;
                if( !_elementId.TryGetValue( element, out var id ) ) continue;
                Debug.Log( $"{element} {_maxElementsCountOf[id]+crawler._realMaxCount[j]} = {_maxElementsCountOf[id]} + {crawler._realMaxCount[j]}" );
                _maxElementsCountOf[id] += crawler._realMaxCount[j];
                _elementAvg[id] += crawler._elementAvg[j];
                _elementIds[id,i] = j;
                _namesCache[id] = crawler._namesCache[j];
            }
        }

        _maxElementCount = 0;
        for( int i = 0; i < _elementsCount; i++ ) if( _maxElementsCountOf[i] > _maxElementCount ) _maxElementCount = _maxElementsCountOf[i];
        Debug.Log( $"_maxElementCount:{_maxElementCount}" );

        _elementCountChance = new double[_elementsCount,_maxElementCount+1];
        _countOfAnyChance = new double[_maxElementCount+1];

        for ( int i = 0; i < _elementsCount; i++ )
        {
            ChancesRecursion( i, 0, 1, 0 );
            for( int j = 0; j <= _maxElementCount; j++ )
            {
                _countOfAnyChance[j] += _elementCountChance[i,j];
            }
        }
    }

    private void ChancesRecursion( int elementId, int depth, double chance, int count )
    {
        if( depth >= nestedCrawlers.Count )
        {
            _elementCountChance[ elementId, count ] += chance;
            return;
        }
        var id = _elementIds[elementId,depth];
        if( id < 0 )
        {
            ChancesRecursion( elementId, depth + 1, chance, count );
            return;
        }
        var crawler = nestedCrawlers[depth];
        var min = crawler._realMinCount[id];
        var max = crawler._realMaxCount[id];
        for( int i = min; i <= max; i++ )
        {
            ChancesRecursion( elementId, depth + 1, chance * crawler._elementCountChance[id,i], count + i );
        }
    }

    public string DebugTimes => $"⏳{ElapsedSeconds:N2}s";
    public string Stringfy( string score = "Score", string count = "Elements Count", string separator = " " ) => _isTooExpensive ? "Time limit exeeded" : $"{score}: {Score}{separator}{count}: {ElementsCount}{separator}Variations(With Permutations): {_variationsCount}({_variationsWithPermutationCount})";
    public override string ToString() => Stringfy();
}
