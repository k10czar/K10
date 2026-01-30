using UnityEngine;
using System.Collections.Generic;
using System;

public class SubsetSelectorPredictor<T>
{
    const float CALC_TIME_LIMIT = 5; 
    public T[] _elements;
    public int[] _countCache;
    public float[] _rollChancesCache;
    public int[] _minCache;
    public int[] _maxCache;
    public float[] _chancesCache;
    // public int[] _scores;
    Func<T,int> _scorer;
    public string[] _namesCache;
    float _sumWeight;
    
    public double[,,] M;
    double[] acc;

    public int rMin;
    public int rMax;
    int count;
    int maxElementsCount;
    int minSum;

    public int Count => count;
    public int PermutationsCount => _permutations.Count;
    public int MaxElementsCount => maxElementsCount;

    public readonly List<int[]> combinations = new();
    public readonly List<double> combinationsChances = new();

    public readonly RangeSummary Score = new();
    public readonly RangeSummary ElementsCount = new();

    List<(string name, float percentage)> _permutations = new();
    public IReadOnlyList<(string name, float percentage)> PermutationsList => _permutations;
    Dictionary<string, float> _permutationChances = new();

    TimeLimit _calculationTimeLimit = new( CALC_TIME_LIMIT * 1000 );
    public ITimeLimit TimeLimit => _calculationTimeLimit;

    public override string ToString()
    {
        return $"Score:{Score} ElementsCount:{ElementsCount} _elements:{_elements.ToStringOrNull()}\ncombinations:{combinations.ToStringOrNull()}\ncombinationsChances:{combinationsChances.ToStringOrNull()}\n_permutations:{_permutations.ToStringOrNull()}";
    }
    

    public void SetScores( Func<T,int> scorer )
    {
        _scorer = scorer;
    }

    public void Calculate( ISubsetSelector<T> set )
    {
        if( set == null ) return;

        _calculationTimeLimit.Begin();

        count = set.EntriesCount;
        FillElements(set);
        FillCaches(set);
        CalculatePredictions(set);

        _calculationTimeLimit.End();
        // Debug.Log( $"Calculate took: {_calculationTimeLimit.ElapsedSeconds()}s" );
    }

    private void FillElements(ISubsetSelector<T> set)
    {
        var count = set.EntriesCount;
        if (_elements == null || _elements.Length != count) _elements = new T[count];
        for (int i = 0; i < set.EntriesCount; i++)
        {
            _elements[i] = set.GetEntry(i).Element;
        }
    }

    public void FillCaches( ISubsetSelector set )
    {
        FillEntriesCaches(set);
        FillRangeWeights(set);
    }

    private void FillRangeWeights(ISubsetSelector set)
    {
        var rCount = set.Max + 1 - set.Min;
        if( !set.IsBiased ) 
        {
            _rollChancesCache = null;
            return;
        }
        else if (_rollChancesCache == null || _rollChancesCache.Length != rCount) _rollChancesCache = new float[rCount];

        for (int i = 0; i < rCount; i++)
        {
            _rollChancesCache[i] = set.GetBiasWeight(set.Min + i);
        }
    }

    public void FillEntriesCaches( ISubsetSelector set )
    {
        var count = set.EntriesCount;

        if (_minCache == null || _minCache.Length != count) _minCache = new int[count];
        if (_maxCache == null || _maxCache.Length != count) _maxCache = new int[count];
        if (_namesCache == null || _namesCache.Length != count) _namesCache = new string[count];
        if (_chancesCache == null || _chancesCache.Length != count) _chancesCache = new float[count];

        _sumWeight = 0f;

        for( int i = 0; i < count; i++ )
        {
            var element = set.GetEntryObject( i );
            _sumWeight += element.Weight;
        }

        minSum = 0;
        maxElementsCount = 0;

        for (int i = 0; i < count; i++)
        {
            var element = set.GetEntryObject( i );
            _chancesCache[i] = element.Weight / _sumWeight;
            var min = element.Guaranteed;
            _minCache[i] = min;
            var max =  element.Cap;
            _maxCache[i] = max;
            var objRef = element.ElementAsObject;
            _namesCache[i] = objRef.DebugNameOrNull();
            minSum += min;
            
            var realCap = Mathf.Max( min, max );
            maxElementsCount = Mathf.Max( maxElementsCount, realCap );
        }

        _rollChancesCache = null;
    }

    public void CalculatePredictions( ISubsetSelector set )
    {
        int count = set.EntriesCount;

        float sumWeight = 0f;

        for( int i = 0; i < count; i++ )
        {
            var element = set.GetEntryObject(i);
            sumWeight += element.Weight;
        }

        CalculatePredictions( set.Min, set.Max, sumWeight );
    }

    public void CalculatePredictions( int minRoll, int maxRoll, float sumWeight )
    {
        rMin = Mathf.Max( minRoll, 0 );
        rMax = Mathf.Max( maxRoll, 0 );

        var maxElementsCountPossible = Mathf.Max( maxElementsCount, rMax );

        M = new double[count, maxElementsCountPossible + 1, rMax + 1];
        CalculateBacktracking( count, _minCache, _maxCache, _chancesCache, rMax, maxElementsCountPossible, M );

        if( _calculationTimeLimit.IsExausted ) return;

        acc = new double[rMax + 1];
        CalculateAcc( count, rMax, maxElementsCountPossible, M, acc );
        
        Permutations();
        
        if( _calculationTimeLimit.IsExausted ) return;
        _permutations.Sort(PermutationSorting);
    }
    

    private static void CalculateAcc( int count, int rMax, int maxElements, double[,,] M, double[] acc )
    {
        for( int i = 0; i < count; i++ )
        {
            for( int k = 1; k <= rMax; k++ )
            {
                for( int j = 0; j <= maxElements; j++ )
                {
                    acc[k] += M[i, j, k];
                }
            }
        }
    }

    private void CalculateBacktracking( int count, int[] min, int[] max, float[] chances, int rMax, int maxElements, double[,,] Predictions )
    {
        var pool = new List<(int id, int max, float chance)>();
        var sequence = new List<int>();
        var elements = new int[count];
        var total = 0;

        var basePercentage = 1f;
        for( int i = 0; i < count; i++ )
        {
            var g = min[i];
            elements[i] = g;
            total += g;
            for( int j = 0; j < g; j++ ) sequence.Add( i );
            var chance = chances[i];
            var addToPool = ( ( max[i] == 0 || g < max[i] ) && chance > 0 );
            if( addToPool ) pool.Add( (i, max[i], chance) );
            else basePercentage -= chance;
        }

        RebasePoolElements( basePercentage, pool );

        for( int i = 0; i <= total && i <= rMax; i++ )
        {
            for( int j = 0; j < count; j++ )
            {
                var c = elements[j];
                Predictions[j, c, i] = 1;
            }
        }

        TryExpand( 1, total, rMax, elements, sequence, pool, Predictions );
    }

    static void RebasePoolElements( float newBase, List<(int id, int max, float chance)> pool )
    {
        for( int i = 0; i < pool.Count; i++ )
        {
            var element = pool[i];
            element.chance /= newBase;
            pool[i] = element;
        }
    }

    void TryExpand( double chance, int total, int maxTotal, int[] elements, List<int> sequence, List<(int id, int max, float chance)> pool, double[,,] M )
    {
        var newTotal = total + 1;
        if( newTotal > maxTotal ) return;

        if( _calculationTimeLimit.CheckExaustion() ) return;

        for( int i = 0; i < pool.Count; i++ )
        {
            var e = pool[i];

            var newCount = elements[e.id] + 1;
            elements[e.id] = newCount;
            var removeFromPool = e.max != 0 && newCount >= e.max;
            if( removeFromPool )
            {
                pool.RemoveAt( i );
                RebasePoolElements( 1 - e.chance, pool );
            }

            chance *= e.chance;
            sequence.Add( e.id );
            // var countStr = string.Join( ", ", System.Array.ConvertAll<int, string>( sequence.ToArray(), ( int v ) => v.ToString() ) );
            // Debug.Log( $"{{ {countStr} }}[{sequence.Count}] => {( chance * 100 ):N1}%" );

            for( int j = 0; j < elements.Length; j++ )
            {
                var c = elements[j];
                M[j, c, newTotal] += chance;
            }

            TryExpand( chance, newTotal, maxTotal, elements, sequence, pool, M );

            sequence.RemoveAt( sequence.Count - 1 );
            chance /= e.chance;
            elements[e.id] = newCount - 1;
            if( removeFromPool )
            {
                RebasePoolElements( 1 / ( 1 - e.chance ), pool );
                pool.Insert( i, e );
            }
        }
    }

    private int PermutationSorting((string, float) x, (string, float) y) => y.Item2.CompareTo( x.Item2 );

    void Permutations()
    {
        var rollsCount = rMax + 1 - rMin;
        var defaultChance = 1f / rollsCount;
        var rChanLen = _rollChancesCache?.Length ?? 0;
        if( rChanLen > rollsCount ) rChanLen = rollsCount;
        var biased = rChanLen > 0;

        if( _countCache == null || _countCache.Length < count ) _countCache = new int[count];

        var baseMins = 0;
        for (int i = 0; i < _minCache.Length; i++)
        {
            baseMins += _minCache[i];
            _countCache[i] = _minCache[i];
        }

        _permutationChances.Clear();

        var rwSum = 0f;
        for (int i = 0; i < rChanLen; i++) rwSum += _rollChancesCache[i];

        Score.StartAccumulator();
        ElementsCount.StartAccumulator();

        combinations.Clear();
        combinationsChances.Clear();

        for( int i = 0; i < rollsCount && !_calculationTimeLimit.IsExausted; i++ )
        {
            var rolls = i + rMin;
            var chance = defaultChance;
            if( biased ) chance = _rollChancesCache[ Mathf.Min( i, rChanLen - 1 )] / rwSum;
            Permute(baseMins, rolls, chance );
        }

        _permutations.Clear();

        if( _calculationTimeLimit.IsExausted ) return;

        foreach( var e in _permutationChances ) _permutations.Add( ( e.Key, e.Value ) );
        _permutationChances.Clear();
    }

    int CalculateScore()
    {
        int score = 0;
        for( int i = 0; i < _countCache.Length; i++ )
        {
            score += _countCache[i];
            if( _scorer != null ) score *= _scorer(_elements[i]);
        }
        return score;
    }
    
    void Permute( int it, int maxPermute, float currChance )
    {
        if( _calculationTimeLimit.CheckExaustion() ) return;
        if( it >= maxPermute )
        {
            var name = NameCombination(_countCache, _namesCache, it);
            _permutationChances.TryGetValue(name, out var chance);

            var realChance = chance + currChance;
            _permutationChances[name] = realChance;
            
            Score.RegisterValue( CalculateScore(), currChance );
            ElementsCount.RegisterValue( maxPermute, currChance );

            var combArray = new int[_countCache.Length];
            for( int i = 0; i < _countCache.Length; i++ ) combArray[i] = _countCache[i];
            combinations.Add( combArray );
            combinationsChances.Add( currChance );

            return;
        }
        var sumWeights = 0f;

        var count = _maxCache.Length;
        for( int k = 0; k < count; k++ )
        {
            if (_maxCache[k] <= _countCache[k]) continue;
            var eChan = _chancesCache[k];
            sumWeights += eChan;
        }
        
        for( int k = 0; k < count; k++ )
        {
            if (_maxCache[k] <= _countCache[k]) continue;
            var eChan = _chancesCache[k];
            _countCache[k]++;
            Permute( it + 1, maxPermute, currChance * ( eChan / sumWeights ) );
            _countCache[k]--;
        }
    }

    private string NameCombination( int[] _countCache, string[] names, int maxRolls = int.MaxValue)
    {
        var sb = StringBuilderPool.RequestEmpty();

        var first = true;
        for (int i = 0; i < _countCache.Length; i++)
        {
            var count = Mathf.Min( _countCache[i], maxRolls );
            if (count > 0)
            {
                if( !first ) sb.Append( ", " );
                sb.Append($"{count} {names[i]}");
                first = false;
            }
            maxRolls -= count;
        }

        if( first ) sb.Append( "NOTHING" );

        return sb.ReturnToPoolAndCast();
    }
}
