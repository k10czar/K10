
using UnityEngine;

/// <summary>
/// Instant framerate from the unscaled frame time, so time scale changes don't skew the reading.
/// </summary>
public class FpsSampler : IStatisticsSampler
{
    public string Label => "FPS";
    public string Format => "0.0";

    public bool IsValid => true;

    public void Start() { }
    public void Stop() { }

    public bool TryGetSample(out float sample)
    {
        sample = 0;
        var deltaTime = Time.unscaledDeltaTime;
        if (deltaTime <= 0) return false;

        sample = 1f / deltaTime;
        return true;
    }
}
