using System.Collections.Generic;
using System;

public class AggregatedPredictor<T>
{
    public List<SubsetSelectorPredictor<T>> nestedCrawlers = new();

    public RangeSummary Score = new();
    public RangeSummary ElementsCount = new();

    bool _isTooExpensive = false;
    public bool IsTooExpensive => _isTooExpensive;

    double _elapsedSeconds = 0;
    public double ElapsedSeconds => _elapsedSeconds;

    public void Calculate( IAggregatedSubsetSelector<T> agg, Func<T,int> Scorer = null )
    {
        nestedCrawlers.Clear();

        Score.SetZero();
        ElementsCount.SetZero();
        _isTooExpensive = false;

        _elapsedSeconds = 0;

        if( agg == null ) return;

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
        }
    }

    public string Stringfy( string score = "Score", string count = "Elements Count", string separator = " " ) => _isTooExpensive ? "Time limit exeeded" : $"{score}: {Score}{separator}{count}: {ElementsCount}";
    public override string ToString() => Stringfy();
}
