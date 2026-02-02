using UnityEngine;
using System.Collections.Generic;
using System;

public class SubsetSelectorPredictor<T>
{
    const float CALC_TIME_LIMIT = 1; 
    public T[] _elements;
    public int[] _countSimilarCache;
    public float[] _rollChancesCache;
    public int[] _minCache;
    public int[] _maxCache;
    public int[] _realMaxCount;
    public int[] _realMinCount;
    public float[] _chancesCache;
    public double[] _scores;
    Func<T,double> _scorer;
    public string[] _namesCache;
    float _sumWeight;

    double[,] _elementCountChance;
    double[] _elementAvg;
    
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
    
    private ulong _variationsCount;
    public ulong VariationsCount => _variationsCount;
    private ulong _variationsWithPermutationCount;
    public ulong VariationsWithPermutationCount => _variationsWithPermutationCount;

    public ITimeLimit TimeLimit => _calculationTimeLimit;

    public IEnumerator<(T element,(double min, double avg, double max)range)> GetElementAveragesEnumerator()
    {
        for( int i = 0; i < count; i++ ) 
        {
            yield return ( _elements[i], ( _realMinCount[i], _elementAvg[i], _realMaxCount[i] ) );
        }
    }

    public override string ToString()
    {
        return $"Score:{Score} ElementsCount:{ElementsCount} _elements:{_elements.ToStringOrNull()}\ncombinations:{combinations.ToStringOrNull()}\ncombinationsChances:{combinationsChances.ToStringOrNull()}\n_permutations:{_permutations.ToStringOrNull()}";
    }
    

    public void SetScores( Func<T,double> scorer )
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
        Debug.Log( $"Calculate {_calculationTimeLimit} {ToString()} on {set}" );
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
        if (_scores == null || _scores.Length != count) _scores = new double[count];
        if (_elementAvg == null || _elementAvg.Length != count) _elementAvg = new double[count];
        if (_realMaxCount == null || _realMaxCount.Length != count) _realMaxCount = new int[count];
        if (_realMinCount == null || _realMinCount.Length != count) _realMinCount = new int[count];
        
        _sumWeight = 0f;

        for( int i = 0; i < count; i++ )
        {
            var element = set.GetEntryObject( i );
            _sumWeight += element.Weight;
            _scores[i] = ( _scorer != null ) ? _scorer(_elements[i]) : 1;
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
            _realMaxCount[i] = min;
            _realMinCount[i] = max;
            var objRef = element.ElementAsObject;
            _namesCache[i] = objRef.DebugNameOrNull();
            minSum += min;
            
            var realCap = Mathf.Max( min, max );
            maxElementsCount = Mathf.Max( maxElementsCount, realCap );
        }
        
        if( _elementCountChance == null || _elementCountChance.GetLength(0) != count || _elementCountChance.GetLength(1) != maxElementsCount + 1 ) _elementCountChance = new double[count,maxElementsCount+1];

        _rollChancesCache = null;
    }

    public void CalculatePredictions( ISubsetSelector set )
    {
        NewCalculatePredictions( set.Min, set.Max );
    }

    public void NewCalculatePredictions( int minRoll, int maxRoll )
    {
        rMin = Mathf.Max( minRoll, 0 );
        rMax = Mathf.Max( maxRoll, 0 );

        var maxElementsCountPossible = Mathf.Max( maxElementsCount, rMax );

        var jugs = new int[count];
        int guaranteeds = 0;
        double guaranteedScore = 0;

        if( _countSimilarCache == null || _countSimilarCache.Length < count ) _countSimilarCache = new int[count];

        for( int i = 0; i < count; i++ )
        {
            var max = _maxCache[i];
            if( max > maxElementsCountPossible ) max  = maxElementsCountPossible;
            var min = _minCache[i];
            guaranteeds += min;
            var delta = max - min;
            guaranteedScore += min * _scores[i];
            jugs[i] = delta;
            _countSimilarCache[i] = 0;
            for( int j = 0; j < maxElementsCount; j++ ) _elementCountChance[i,j] = 0;
        }
        
        var realMin = rMin - guaranteeds;
        var realMax = rMax - guaranteeds;

        if( realMax <= 0 )
        {
            Score.SetOnlyOne( guaranteedScore );
            ElementsCount.SetOnlyOne( guaranteeds );
            _variationsCount = 1;
            _variationsWithPermutationCount = 1;
            return;
        }

        var startRoll = realMin;
        if( realMin < 0 ) startRoll = 0;

        Score.StartAccumulator();
        ElementsCount.StartAccumulator();
        _variationsCount = 0;
        _variationsWithPermutationCount = 0;
        var baseChance = 1;
        if( realMax > startRoll ) baseChance = realMax + 1 - startRoll;
        for( int i = startRoll; i <= realMax; i++ )
        {
            Debug.Log( $"TryRool: {i+guaranteeds}" );
            var rollChance = _rollChancesCache != null ? _rollChancesCache[i + guaranteeds] : baseChance;
            Generate( guaranteedScore, guaranteeds, rollChance, jugs, _scores, _chancesCache, i, 0, count, Score );
            ElementsCount.RegisterValue( guaranteeds + i, rollChance );
        }
        Score.Normalize();
        ElementsCount.Normalize();

        CalculateAverages();
    }

    private void CalculateAverages()
    {
        for( int i = 0; i < count; i++ )
        {
            _elementAvg[i] = 0;
            for( int j = 1; j <= maxElementsCount; j++ )
            {
                _elementAvg[i] += j * _elementCountChance[i,j];
                // Debug.Log( $"{i} +{j * _elementCountChance[i,j]} = {j} * {_elementCountChance[i,j]}" );
            }
        }
    }

    ulong PermutationsOfCountCache()
    {
        var biggestJug = 0;
        var biggestJugValue = _countSimilarCache[0];
        var sumOfElements = biggestJugValue;
        for( int i = 1; i < _countSimilarCache.Length; i++ )
        {
            var elements = _countSimilarCache[i];
            sumOfElements += elements;
            if( biggestJugValue >= elements ) continue;
            biggestJug = i;
            biggestJugValue = elements;
        }
        // Debug.Log( $"PermutationsOfCountCache() ( {sumOfElements}! / !!!{_countCache.ToStringOrNull()}!!! )" );
        ulong permutations = 1;
        for( int i = biggestJugValue + 1; i <= sumOfElements; i++ )
        {
            permutations *= (ulong)i;
        }
        for( int i = 1; i < _countSimilarCache.Length; i++ )
        {
            if( i == biggestJug ) continue;
            permutations /= K10.Math.Factorial( _countSimilarCache[i] );
        }
        return permutations;
    }

    void Generate( double score, int count, double chance, int[] limits, double[] scores, float[] chances, int remaining, int startIndex, int length, RangeSummary scoreSummary )
    {
        if (remaining == 0)
        {
            var permutations = PermutationsOfCountCache();
            var realChance = chance * permutations;
            for( int i = 0; i < _countSimilarCache.Length; i++ ) 
            {
                var eCount = _countSimilarCache[i];
                var realCount = eCount + _minCache[i];
                if( realCount > _realMaxCount[i] ) _realMaxCount[i] = realCount; 
                if( realCount < _realMinCount[i] ) _realMinCount[i] = realCount;
                _elementCountChance[i,realCount] += realChance;
            }
            Debug.Log( $"{NameCombination(_countSimilarCache,_namesCache)} XP add {score} {permutations*chance*100:N4}% ( {permutations} * {chance*100:N4}% )" );
            scoreSummary.RegisterValue( score, realChance );
            _variationsCount++;
            _variationsWithPermutationCount += permutations;
            return;
        }

        for (int i = startIndex; i < length; i++)
        {
            if (limits[i] == 0)
                continue;

            limits[i]--;
            _countSimilarCache[i]++;
            Generate(score + scores[i], count + 1, chance * chances[i], limits, scores, chances, remaining - 1, i, length, scoreSummary);
            _countSimilarCache[i]--;
            limits[i]++;
        }
    }

    private static string NameCombination( int[] _countCache, string[] names)
	{
		var sb = StringBuilderPool.RequestEmpty();

		var first = true;
		for (int i = 0; i < _countCache.Length; i++)
		{
			var count = _countCache[i];
			if (count > 0)
			{
				if( !first ) sb.Append( ", " );
				sb.Append($"{count} {names[i]}");
				first = false;
			}
		}

		if( first ) sb.Append( "NOTHING" );

		return sb.ReturnToPoolAndCast();
    }
}
