
/// <summary>
/// A single metric source feeding a <see cref="RunningStatistics"/>.
/// Implementations own their lifetime (profiler recorders, etc) but know nothing about display.
/// </summary>
public interface IStatisticsSampler
{
    string Label { get; }

    /// <summary>Numeric format used when displaying the values of this metric.</summary>
    string Format { get; }

    /// <summary>False when the metric cannot be read at all on the current build/platform.</summary>
    bool IsValid { get; }

    void Start();
    void Stop();

    /// <summary>Reads the metric for this frame, false when there is nothing usable to accumulate.</summary>
    bool TryGetSample(out float sample);
}
