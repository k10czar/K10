using System.Collections.Generic;
using System;

public interface IAggregatedPredictor<T>
{
    void Calculate(IAggregatedSubsetSelector<T> agg);
}

public class AggregatedPredictor<T> : BaseAggregatedPredictor<T>
{
    public List<SubsetSelectorPredictor<T>> nestedCrawlers = new();
    Func<T, double> _scorer;

    public override int SubPredictorCount => nestedCrawlers?.Count ?? 0;
    public override BaseSubsetSelectorPredictor<T> GetSubPredictor(int index) => nestedCrawlers != null && nestedCrawlers.Count > index ? nestedCrawlers[index] : null;
    protected override void ClearCrawlers()
    {
        nestedCrawlers.Clear();
    }
    public void SetScorer(Func<T, double> scorer)
    {
        _scorer = scorer;
    }
    protected override BaseSubsetSelectorPredictor<T> BuildCrawler(ISubsetSelector<T> subsetSelector)
    {
        var crawler = new SubsetSelectorPredictor<T>();
        crawler.SetScorer( _scorer );
        crawler.Calculate( subsetSelector );
        nestedCrawlers.Add( crawler );
        return crawler;
    }
}
