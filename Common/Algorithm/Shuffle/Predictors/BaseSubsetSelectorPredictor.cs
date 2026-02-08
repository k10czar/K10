using UnityEngine;
using System.Collections.Generic;

public abstract class BaseSubsetSelectorPredictor<T>
{
    const float CALC_TIME_LIMIT = 1; 
    public T[] _elements;
    public int[] _countSimilarCache;
    public double[] _rollChancesCache;
    public int[] _minCache;
    public int[] _maxCache;
    public float[] _chancesCache;
    public string[] _namesCache;
    float _sumWeight;

    public int[] _realMaxCount;
    public int[] _realMinCount;
    public double[,] _elementCountChance;
    public double[] _elementAvg;
    
    public double[] _countOfAnyChance;

    public int rMin;
    public int rMax;
    protected int count;
    int maxElementsCount;
    int minSum;

    public int Count => count;
    public int PermutationsCount => _permutations.Count;
    public int MaxElementsCount => maxElementsCount;

    protected double totalChances = 0;

    // public readonly List<int[]> combinations = new();
    // public readonly List<double> combinationsChances = new();

    public readonly RangeSummary Score = new();
    public readonly RangeSummary ElementsCount = new();

    List<(string name, float percentage)> _permutations = new();
    public IReadOnlyList<(string name, float percentage)> PermutationsList => _permutations;
    Dictionary<string, float> _permutationChances = new();

    protected TimeLimit _calculationTimeLimit = new( CALC_TIME_LIMIT * 1000 );
    
    protected ulong _variationsCount;
    public ulong VariationsCount => _variationsCount;
    protected ulong _variationsWithPermutationCount;
    public ulong VariationsWithPermutationCount => _variationsWithPermutationCount;

    public bool HasChanceOfAnyElementCount( int count ) => _countOfAnyChance[count] > float.Epsilon;

    public ITimeLimit TimeLimit => _calculationTimeLimit;

    public IEnumerator<(T element,(double min, double avg, double max)range)> GetElementsRangeEnumerator()
    {
        for( int i = 0; i < count; i++ ) 
        {
            yield return ( _elements[i], ( _realMinCount[i], _elementAvg[i], _realMaxCount[i] ) );
        }
    }

    // public override string ToString()
    // {
    //     return $"Score:{Score} ElementsCount:{ElementsCount} _elements:{_elements.ToStringOrNull()}\ncombinations:{combinations.ToStringOrNull()}\ncombinationsChances:{combinationsChances.ToStringOrNull()}\n_permutations:{_permutations.ToStringOrNull()}";
    // }

    protected abstract void BuildScores( int count );
    

    public void Calculate( ISubsetSelector<T> set )
    {
        if( set == null ) return;

        _calculationTimeLimit.Begin();

        count = set.EntriesCount;
        FillCaches(set);
        CalculatePredictions(set);

        _calculationTimeLimit.End();
        // Debug.Log( $"Calculate {_calculationTimeLimit} {ToString()} on {set}" );
    }

    public void FillCaches( ISubsetSelector set )
    {
        var count = set.EntriesCount;

        if (_minCache == null || _minCache.Length != count) _minCache = new int[count];
        if (_maxCache == null || _maxCache.Length != count) _maxCache = new int[count];
        if (_namesCache == null || _namesCache.Length != count) _namesCache = new string[count];
        if (_chancesCache == null || _chancesCache.Length != count) _chancesCache = new float[count];
        if (_elementAvg == null || _elementAvg.Length != count) _elementAvg = new double[count];
        if (_realMaxCount == null || _realMaxCount.Length != count) _realMaxCount = new int[count];
        if (_realMinCount == null || _realMinCount.Length != count) _realMinCount = new int[count];
        if (_elements == null || _elements.Length != count) _elements = new T[count];
        
        _sumWeight = 0f;

        for( int i = 0; i < count; i++ )
        {
            var element = set.GetEntryObject( i );
            _elements[i] = (T)element.ElementAsObject;
            _sumWeight += element.Weight;
        }
        
        BuildScores( count );

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

        if( _countOfAnyChance == null || _countOfAnyChance.Length != maxElementsCount + 1 ) _countOfAnyChance = new double[maxElementsCount+1];
        if( _elementCountChance == null || _elementCountChance.GetLength(0) != count || _elementCountChance.GetLength(1) != maxElementsCount + 1 ) _elementCountChance = new double[count,maxElementsCount+1];

        _rollChancesCache = null;
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
        
        if (_rollChancesCache == null || _rollChancesCache.Length != rCount) _rollChancesCache = new double[rCount];

        var biasSum = 0.0;
        for (int i = 0; i < rCount; i++)
        {
            var bias = set.GetBiasWeight(set.Min + i);
            biasSum += bias;
            _rollChancesCache[i] = bias;
        }

        if( biasSum < 2 * double.Epsilon ) return;

        for (int i = 0; i < rCount; i++)
        {
            _rollChancesCache[i] /= biasSum;
        }
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
        ResetGuaranteedScore();

        if( _countSimilarCache == null || _countSimilarCache.Length < count ) _countSimilarCache = new int[count];

        for( int i = 0; i < count; i++ )
        {
            var max = _maxCache[i];
            if( max > maxElementsCountPossible ) max  = maxElementsCountPossible;
            var min = _minCache[i];
            guaranteeds += min;
            var delta = max - min;
            AddGuaranteedScore( min, i );
            jugs[i] = delta;
            _countSimilarCache[i] = 0;
            for( int j = 0; j < maxElementsCount; j++ ) _elementCountChance[i,j] = 0;
        }
        
        var realMin = rMin - guaranteeds;
        var realMax = rMax - guaranteeds;

        if( realMax <= 0 )
        {
            ScoreOnlyGuaranteeds();
            ElementsCount.SetOnlyOne( guaranteeds );
            _variationsCount = 1;
            _variationsWithPermutationCount = 1;
            for( int i = 0; i < count; i++ )
            {
                var countOfElement = _minCache[i];
                _realMinCount[i] = countOfElement;
                _elementAvg[i] = countOfElement;
                _realMaxCount[i] = countOfElement;
                _elementCountChance[i,_minCache[i]] = 1;
                _countOfAnyChance[countOfElement] = 1;
            }
            return;
        }

        var startRoll = realMin;
        if( realMin < 0 ) startRoll = 0;

        totalChances = 0;

        PreGenerateCases();

        ElementsCount.StartAccumulator();
        _variationsCount = 0;
        _variationsWithPermutationCount = 0;
        var baseChance = 1.0;
        if( realMax > startRoll ) baseChance = 1.0 / ( realMax + 1 - startRoll );
        for( int i = startRoll; i <= realMax; i++ )
        {
            // Debug.Log( $"TryRool: {i+guaranteeds}" );
            var realRollsCount = i+guaranteeds;
            var rollChance = _rollChancesCache != null ? _rollChancesCache[realRollsCount-rMin] : baseChance;
            GenerateCase( i, rollChance, jugs );
            ElementsCount.RegisterValue( realRollsCount, rollChance );
        }

        PostGenerateCases();
        ElementsCount.Normalize();

        CalculateAverages();
    }

    protected abstract void GenerateCase(int rolls, double rollChance, int[] elementsPool);
    protected abstract void ResetGuaranteedScore();
    protected abstract void AddGuaranteedScore(int quantity, int elementId);
    protected abstract void ScoreOnlyGuaranteeds();
    protected abstract void PostGenerateCases();
    protected abstract void PreGenerateCases();

    private void CalculateAverages()
    {
        var mult = 1.0;
        if( totalChances > float.Epsilon ) mult = 1.0 / totalChances;
        for( int i = 0; i < count; i++ )
        {
            _elementAvg[i] = 0;
            for( int j = 1; j <= maxElementsCount; j++ )
            {
                var chance = _elementCountChance[i,j] * mult;
                _elementCountChance[i,j] = chance;
                _elementAvg[i] += j * chance;
                _countOfAnyChance[j] += chance;
            }
            Debug.Log( $"{i}){_elements[i].ToStringOrNull()} avg:{_elementAvg[i]}" );
        }
    }

    protected ulong PermutationsOfCountCache()
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

    protected string NameScratchCombination()
    {
		var sb = StringBuilderPool.RequestEmpty();

		var first = true;
		for (int i = 0; i < _countSimilarCache.Length; i++)
		{
			var count = _countSimilarCache[i] + _minCache[i];
			if (count > 0)
			{
				if( !first ) sb.Append( ", " );
				sb.Append($"{count} {_namesCache[i]}");
				first = false;
			}
		}

		if( first ) sb.Append( "NOTHING" );

		return sb.ReturnToPoolAndCast();
    }

    protected static string NameCombination( int[] _countCache, string[] names)
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
