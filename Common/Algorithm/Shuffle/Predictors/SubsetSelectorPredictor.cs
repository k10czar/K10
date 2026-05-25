using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using UnityEngine;

public class SubsetSelectorPredictor<T> : BaseSubsetSelectorPredictor<T>
{
    public double[] _scores;
    Func<T,double> _scorer;

    double guaranteedScore = 0;
    int guaranteeds = 0;
    
    public void SetScorer( Func<T,double> scorer )
    {
        _scorer = scorer;
    }

    protected override void BuildScores( int count )
    {
        if (_scores == null || _scores.Length != count) _scores = new double[count];


        for( int i = 0; i < count; i++ )
        {
            _scores[i] = ( _scorer != null ) ? _scorer(_elements[i]) : 1;
        }
    }

    protected override void ResetGuaranteedScore()
    {
        guaranteeds = 0;
        guaranteedScore = 0;
    }

    protected override void AddGuaranteedScore(int quantity, int elementId)
    {
        guaranteeds += quantity;
        guaranteedScore += quantity * _scores[elementId];
    }

    protected override void PreGenerateCases()
    {
        Score.StartAccumulator();
    }

    protected override void PostGenerateCases()
    {
        Score.Normalize();
    }

    protected override void ScoreOnlyGuaranteeds()
    {
        Score.SetOnlyOne( guaranteedScore );
    }

    protected override void GenerateCase( int rolls, double rollChance, int[] elementsPool )
    {
        Generate( guaranteedScore, guaranteeds, rollChance, elementsPool, rolls, 0, count );
    }

    string NameCacheCombination()
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

    void Generate( double score, int count, double chance, int[] limits, int remaining, int startIndex, int length )
    {
        if (remaining == 0)
        {
            if( _calculationTimeLimit.CheckExaustion() ) return;
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
            totalChances += realChance;
            Score.RegisterValue( score, realChance );
            _variationsCount++;
            _variationsWithPermutationCount += permutations;
            return;
        }

        for (int i = startIndex; i < length && !_calculationTimeLimit.IsExausted; i++)
        {
            if (limits[i] == 0)
                continue;

            limits[i]--;
            _countSimilarCache[i]++;
            Generate(score + _scores[i], count + 1, chance * _chancesCache[i], limits, remaining - 1, i, length);
            _countSimilarCache[i]--;
            limits[i]++;
        }
    }
}
