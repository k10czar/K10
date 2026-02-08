using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class BaseAggregatedPredictor<T> : IAggregatedPredictor<T>
{
    public RangeSummary Score = new();
    public RangeSummary ElementsCount = new();

    protected bool _isTooExpensive = false;
    double _elapsedSeconds = 0;
    ulong _variationsCount = 0;
    ulong _variationsWithPermutationCount = 0;

    public bool IsTooExpensive => _isTooExpensive;
    public double ElapsedSeconds => _elapsedSeconds;
    public ulong VariationsCount => _variationsCount;
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

    Dictionary<T,RangeSummary> _elementRange = new();

    public IEnumerator<KeyValuePair<T,RangeSummary>> GetElementAveragesEnumerator() => _elementRange.GetEnumerator();

    public abstract int SubPredictorCount { get; }
    public abstract BaseSubsetSelectorPredictor<T> GetSubPredictor(int index);
    protected abstract void ClearCrawlers();
    protected abstract BaseSubsetSelectorPredictor<T> BuildCrawler(ISubsetSelector<T> subsetSelector);

    public bool HasChanceOfAnyElementCount(int count)
    {
        return _countOfAnyChance != null && _countOfAnyChance[count] > float.Epsilon;
    }

    public void Calculate( IAggregatedSubsetSelector<T> agg, Func<T,double> Scorer = null )
    {
        Score.SetZero();
        ElementsCount.SetZero();
        _isTooExpensive = false;
        foreach( var range in _elementRange ) range.Value.SetZero();

        _elapsedSeconds = 0;
        ClearCrawlers();

        if( agg == null ) return;

        _variationsCount = 1;
        _variationsWithPermutationCount = 1;

        for ( int i = 0; i < agg.Count && !_isTooExpensive; i++ )
        {
            var crawler = BuildCrawler( agg.GetEntry(i) );
            _elapsedSeconds += crawler.TimeLimit.ElapsedSeconds();

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
                if( element == null ) continue;
                if( !_elementRange.TryGetValue( element, out var range ) ) 
                {
                    range = new();
                    range.SetZero();
                    _elementRange[element] = range;
                }
                range.Combine( entryRange );
            }
        }

        var subCount = SubPredictorCount;
        for ( int i = 0; i < subCount; i++ )
        {
            var crawler = GetSubPredictor(i);
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

        _elementIds = new int[_elementsCount,subCount];

        for ( int i = 0; i < subCount; i++ )
        {
            for ( int j = 0; j < _elementsCount; j++ )
            {
                _elementIds[j,i] = -1;
            }

            var crawler = GetSubPredictor(i);
            var count = crawler._elements.Length;
            for ( int j = 0; j < count; j++ )
            {
                var element = crawler._elements[j];
                if( element == null ) continue;
                if( !_elementId.TryGetValue( element, out var id ) ) continue;
                _maxElementsCountOf[id] += crawler._realMaxCount[j];
                _elementAvg[id] += crawler._elementAvg[j];
                _elementIds[id,i] = j;
                _namesCache[id] = crawler._namesCache[j];
            }
        }

        _maxElementCount = 0;
        for( int i = 0; i < _elementsCount; i++ ) if( _maxElementsCountOf[i] > _maxElementCount ) _maxElementCount = _maxElementsCountOf[i];

        _elementCountChance = new double[_elementsCount,_maxElementCount+1];
        _countOfAnyChance = new double[_maxElementCount+1];

        _debugRecursion = new int[subCount];
        _debugRecursionChances = new float[subCount];

        for ( int i = 0; i < _elementsCount; i++ )
        {
            ChancesRecursion( i, 0, 1, 0 );
            for( int j = 0; j <= _maxElementCount; j++ )
            {
                _countOfAnyChance[j] += _elementCountChance[i,j];
            }
        }
    }

    int[] _debugRecursion;
    float[] _debugRecursionChances;

    private void ChancesRecursion( int elementId, int depth, double chance, int count )
    {
        if( depth >= SubPredictorCount )
        {
            _elementCountChance[ elementId, count ] += chance;
            // Debug.Log( $"{elementId} {_debugRecursion.ElementsToString()}({_debugRecursionChances.ElementsToString()}) {count} {chance*100:N2}%" );
            return;
        }
        var id = _elementIds[elementId,depth];
        _debugRecursion[depth] = id;
        if( id < 0 )
        {
            ChancesRecursion( elementId, depth + 1, chance, count );
            _debugRecursionChances[depth] = 1;
            return;
        }
        var crawler = GetSubPredictor(depth);
        var min = crawler._realMinCount[id];
        var max = crawler._realMaxCount[id];
        for( int i = min; i <= max; i++ )
        {
            var eChance = crawler._elementCountChance[id,i];
            _debugRecursionChances[depth] = (float)eChance;
            ChancesRecursion( elementId, depth + 1, chance * eChance, count + i );
        }
    }

    public string DebugTimes => $"⏳{ElapsedSeconds:N2}s";
    public virtual string Stringfy( string score = "Score", string count = "Elements Count", string separator = " " ) => _isTooExpensive ? "Time limit exeeded" : $"{score}: {Score}{separator}{count}: {ElementsCount}{separator}Variations(With Permutations): {_variationsCount}({_variationsWithPermutationCount})";
    public override string ToString() => Stringfy();
}
