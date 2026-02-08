using System.Collections.Generic;
using System;
using UnityEngine;

public class CompoundSubsetSelectorPredictor<T,K> : BaseSubsetSelectorPredictor<T> where T : IAggregatedSubsetSelector<K>
{
    public (double min,double avg,double max)[] _scores;
    Func<K,double> _scorerK;

    (double min,double avg,double max) guaranteedScore = (0,0,0);
    int guaranteeds = 0;
    
    Dictionary<T,AggregatedPredictor<K>> _crawlersDict = new();
    AggregatedPredictor<K>[] _crawlers;

    protected ulong _subElementVariationsCount;
    public ulong SubElementVariationsCount => _subElementVariationsCount;
    protected ulong _subElementVariationsWithPermutationCount;
    public ulong SubElementVariationsWithPermutationCount => _subElementVariationsWithPermutationCount;

    public readonly RangeSummary SubElementsCountRange = new();
    RangeSummary[] _subElementsRanges;
    RangeSummary _subElementsCountRangeScratch = new();
    RangeSummary[] _subElementsRangeScratch;
    
    readonly Dictionary<K,int> _subElementsId = new();
    K[] _subElements;
    int _subElementsCount = 0;

    public IEnumerator<(K element,RangeSummary range)> GetSubElementsRangeEnumerator()
    {
        for( int i = 0; i < _subElementsCount; i++ ) 
        {
            yield return ( _subElements[i], _subElementsRanges[i] );
        }
    }
    
    public void SetScorer( Func<K,double> scorer )
    {
        _scorerK = scorer;
    }

    protected override void BuildScores( int count )
    {
        if (_scores == null || _scores.Length != count) _scores = new (double, double, double)[count];
        if (_crawlers == null || _crawlers.Length != count) _crawlers = new AggregatedPredictor<K>[count];


        for (int i = 0; i < count; i++)
        {
            var crawler = RequestCrawler(_elements[i]);
            _crawlers[i] = crawler;
            _scores[i] = crawler?.Score.ToTupple() ?? ( 0,0,0 );
        }

        _subElementsCount = _subElementsId.Count;
        TryStartRangeArray( ref _subElementsRangeScratch, _subElementsCount );
        TryStartRangeArray( ref _subElementsRanges, _subElementsCount );

        if (_subElements == null || _subElements.Length != _subElementsCount) _subElements = new K[_subElementsCount];
        foreach( var kvp in _subElementsId) _subElements[kvp.Value] = kvp.Key;
    }

    private static void TryStartRangeArray( ref RangeSummary[] collection, int count )
    {
        if (collection == null || collection.Length != count)
        {
            collection = new RangeSummary[count];
            for (int i = 0; i < count; i++) collection[i] = new RangeSummary();
        }
    }

    private AggregatedPredictor<K> RequestCrawler(T t)
    {
        if( t == null ) return null;
        if (!_crawlersDict.TryGetValue(t, out var crawler))
        {
            crawler = new AggregatedPredictor<K>();
            crawler.Calculate(t, _scorerK);
            _crawlersDict.Add(t, crawler);
            for( int i = 0; i < t.Count; i++ )
            {
                var e = t.GetEntry( i );
                for( int j = 0; j < e.EntriesCount; j++ )
                {
                    var entry = e.GetEntry( j ).Element;
                    if( entry == null ) continue;
                    if( _subElementsId.ContainsKey( entry ) ) continue;
                    var keysCount = _subElementsId.Count;
                    _subElementsId[entry] = keysCount;
                    _subElementsCount = keysCount + 1;
                }
            }
        }

        return crawler;
    }

    protected override void ResetGuaranteedScore()
    {
        guaranteeds = 0;
        guaranteedScore = (0,0,0);
    }

    protected override void AddGuaranteedScore(int quantity, int elementId)
    {
        guaranteeds += quantity;
        var elementScore = _scores[elementId];
        guaranteedScore = ( guaranteedScore.min + quantity * elementScore.min,
                            guaranteedScore.avg + quantity * elementScore.avg,
                            guaranteedScore.max + quantity * elementScore.max );
    }

    protected override void PreGenerateCases()
    {
        SubElementsCountRange.StartAccumulator();
        Score.StartAccumulator();
        for( int i = 0; i < _subElementsRanges.Length; i++ ) _subElementsRanges[i].StartAccumulator();
    }

    protected override void PostGenerateCases()
    {
        Score.Normalize();
        for( int i = 0; i < _subElementsRanges.Length; i++ ) _subElementsRanges[i].Normalize(); 
        SubElementsCountRange.Normalize();
    }

    protected override void ScoreOnlyGuaranteeds()
    {
        Score.SetOnlyOne(guaranteedScore.min, guaranteedScore.avg, guaranteedScore.max);
        ResetElementsRangeScratch();
        SubElementsCountRange.SetZero();
        _subElementVariationsCount = 1;
        _subElementVariationsWithPermutationCount = 1;
        for (int i = 0; i < count; i++)
        {
            var quantity = _minCache[i];
            if (quantity <= 0) continue;
            var crawler = _crawlers[i];
            if (crawler == null) continue;
            var enumerator = crawler.GetElementAveragesEnumerator();
            while (enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                if( entry.Key == null ) continue;
                var eId = _subElementsId[entry.Key];
                _subElementsRangeScratch[eId].Combine(entry.Value, quantity);
            }
            SubElementsCountRange.Combine( crawler.ElementsCount, quantity );
            _subElementVariationsCount *= crawler.VariationsCount;
            _subElementVariationsWithPermutationCount *= crawler.VariationsWithPermutationCount;
        }
        RegisterSubElementsRangeScratch( 1 );
    }

    private void RegisterSubElementsRangeScratch( double chance )
    {
        for (int i = 0; i < _subElementsCount; i++)
        {
            _subElementsRanges[i].RegisterValue(_subElementsRangeScratch[i], chance);
        }
    }

    protected override void GenerateCase( int rolls, double rollChance, int[] elementsPool )
    {
        Generate( guaranteedScore, guaranteeds, rollChance, elementsPool, rolls, 0, count );
    }

    private void ResetElementsRangeScratch()
    {
        for( int i = 0; i < _subElementsRangeScratch.Length; i++ ) _subElementsRangeScratch[i].SetZero(); 
        _subElementsCountRangeScratch.SetZero();
    }

    void Generate( (double min,double avg,double max) score, int count, double chance, int[] limits, int remaining, int startIndex, int length )
    {
        if (remaining == 0)
        {
            if( _calculationTimeLimit.CheckExaustion() ) return;
            var permutations = PermutationsOfCountCache();
            var realChance = chance * permutations;
            ulong subElementVariation = 1;
            ulong subElementVariationWithPermutation = 1;
            ResetElementsRangeScratch();
            for( int i = 0; i < _countSimilarCache.Length; i++ ) 
            {
                var eCount = _countSimilarCache[i];
                var realCount = eCount + _minCache[i];
                if( realCount > _realMaxCount[i] ) _realMaxCount[i] = realCount; 
                if( realCount < _realMinCount[i] ) _realMinCount[i] = realCount;
                _elementCountChance[i,realCount] += realChance;
                var crawler = _crawlers[i];
                if( crawler == null ) continue;
                if( realCount > 0)
                {
                    var enumerator = crawler.GetElementAveragesEnumerator();
                    while( enumerator.MoveNext() )
                    {
                        var entry = enumerator.Current;
                        if( entry.Key == null ) continue;
                        var eId = _subElementsId[ entry.Key ];
                        _subElementsRangeScratch[eId].Combine(entry.Value, realCount);
                        Debug.Log( $"{entry.Key} => {entry.Value}" );
                    }
                }
                subElementVariation *= crawler.VariationsCount;
                subElementVariationWithPermutation *= crawler.VariationsWithPermutationCount;
                _subElementsCountRangeScratch.Combine(crawler.ElementsCount, realCount);
            }
            totalChances += realChance;
            RegisterSubElementsRangeScratch( realChance );
            // Debug.Log( $"{NameCombination(_countSimilarCache,_namesCache)} Combine {_subElementsCountRangeScratch}{realChance*100:N2}% from {base.NameScratchCombination()} {totalChances}\n ( {_subElementsRangeScratch.ElementsToString()} ) " );
            SubElementsCountRange.RegisterValue( _subElementsCountRangeScratch, realChance );
            // Debug.Log( $"{NameCombination(_countSimilarCache,_namesCache)} XP add {score} {permutations*chance*100:N4}% ( {permutations} * {chance*100:N4}% )" );
            Score.RegisterValue( score.min, score.avg, score.max, realChance );
            _variationsCount++;
            _variationsWithPermutationCount += permutations;
            _subElementVariationsCount += subElementVariation;
            _subElementVariationsWithPermutationCount += subElementVariationWithPermutation * permutations;
            return;
        }

        for (int i = startIndex; i < length && !_calculationTimeLimit.IsExausted; i++)
        {
            if (limits[i] == 0)
                continue;

            limits[i]--;
            _countSimilarCache[i]++;
            Generate( ( score.min + _scores[i].min, score.avg + _scores[i].avg, score.max + _scores[i].max ), count + 1, chance * _chancesCache[i], limits, remaining - 1, i, length);
            _countSimilarCache[i]--;
            limits[i]++;
        }
    }
}
