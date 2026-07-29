
/// <summary>
/// Binds a <see cref="IStatisticsSampler"/> to its <see cref="RunningStatistics"/> and knows how to
/// report them as text. Owns the sampler lifetime so callers only deal with Start/Stop/Reset/Update.
/// </summary>
public class SampledStatistics
{
    public IStatisticsSampler Sampler { get; }
    public RunningStatistics Statistics { get; } = new RunningStatistics();

    public SampledStatistics(IStatisticsSampler sampler)
    {
        Sampler = sampler;
    }

    public void Start() => Sampler.Start();
    public void Stop() => Sampler.Stop();
    public void Reset() => Statistics.Reset();

    public void Update()
    {
        if (!Sampler.IsValid) return;
        if (!Sampler.TryGetSample(out var sample)) return;
        Statistics.AddSample(sample);
    }

    public string ToReport()
    {
        if (!Sampler.IsValid) return $"{Sampler.Label} unavailable";

        var f = Sampler.Format;
        return $"{Sampler.Label}: {Statistics.Current.ToString(f)} | " +
               $"Avg: {Statistics.Average.ToString(f)}  Min: {Statistics.Min.ToString(f)}  Max: {Statistics.Max.ToString(f)}";
    }
}
