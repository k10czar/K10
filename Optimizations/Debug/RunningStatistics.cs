
/// <summary>
/// Accumulates samples of a single metric, keeping current/average/min/max.
/// Pure data, no Unity dependency, so it can back any sampler.
/// </summary>
public class RunningStatistics
{
    public float Current { get; private set; }
    public float Average { get; private set; }
    public float Min { get; private set; }
    public float Max { get; private set; }
    public int Samples { get; private set; }

    double _sum;

    public void Reset()
    {
        Current = 0;
        Average = 0;
        Min = 0;
        Max = 0;
        Samples = 0;
        _sum = 0;
    }

    public void AddSample(float value)
    {
        Current = value;
        if (Samples == 0 || value < Min) Min = value;
        if (Samples == 0 || value > Max) Max = value;

        _sum += value;
        Samples++;
        Average = (float)(_sum / Samples);
    }
}
