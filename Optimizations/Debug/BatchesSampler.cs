
using Unity.Profiling;

/// <summary>
/// Editor uses the exact Stats-window value; development builds read the profiler counter
/// (stripped from release builds, hence the guards).
/// </summary>
public class BatchesSampler : IStatisticsSampler
{
    public string Label => "Batches";
    public string Format => "0";

    ProfilerRecorder _recorder;

    public bool IsValid
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
            return _recorder.Valid;
#endif
        }
    }

    public void Start()
    {
#if !UNITY_EDITOR && DEVELOPMENT_BUILD
        _recorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
#endif
    }

    public void Stop()
    {
#if !UNITY_EDITOR && DEVELOPMENT_BUILD
        _recorder.Dispose();
#endif
    }

    public bool TryGetSample(out float sample)
    {
        sample = 0;
#if UNITY_EDITOR
        sample = UnityEditor.UnityStats.batches;
#elif DEVELOPMENT_BUILD
        if (!_recorder.Valid) return false;
        sample = _recorder.LastValue;
#else
        return false;
#endif
        // Zero means the frame had nothing rendered yet, not a real measurement.
        return sample > 0;
    }
}
