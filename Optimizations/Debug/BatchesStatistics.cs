
using Unity.Profiling;
using UnityEngine;

public class BatchesStatistics : MonoBehaviour
{
    [SerializeField] Rect _rect = new Rect(10, 10, 320, 70);
    [SerializeField] Color _color = Colors.KeyLime;
    [SerializeField] int _fontSize = 22;

    GUIStyle _style;
    GUIStyle Style
    {
        get
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft,
                };
            }
            _style.fontSize = _fontSize;
            return _style;
        }
    }

    public int _current;
    public int _average;
    public int _min;
    public int _max;

    bool _counting;
    long _sum;
    int _samples;

    ProfilerRecorder _batchesRecorder;

    private static BatchesStatistics _instance;
    public static BatchesStatistics Instance
    {
        get
        {
            if (_instance == null)
            {
                var go =  new GameObject("[BatchesStatistics]");
                _instance = go.AddComponent<BatchesStatistics>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public void Reset()
    {
        _current = 0;
        _average = 0;
        _min = 0;
        _max = 0;
        _sum = 0;
        _samples = 0;
        _counting = true;
    }

    public void Stop()
    {
        _counting = false;
    }

    void OnEnable()
    {
        _batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
    }

    void OnDisable()
    {
        _batchesRecorder.Dispose();
    }

    bool IsValid()
    {
#if UNITY_EDITOR
		return true;
#else
		return _batchesRecorder.Valid;
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	// Editor uses the exact Stats-window value; development builds read the profiler counter (stripped from
	// release builds, hence the guard).
	long GetCurrentBatches()
	{
#if UNITY_EDITOR
		return UnityEditor.UnityStats.batches;
#else
		return _batchesRecorder.Valid ? _batchesRecorder.LastValue : 0;
#endif
	}
#endif

    void Update()
    {
        if (!_counting || !IsValid())
            return;

        var batches = (int)GetCurrentBatches();
        if (batches <= 0)
            return;

        _current = batches;
        if (_samples == 0 || batches < _min) _min = batches;
        if (batches > _max) _max = batches;

        _sum += batches;
        _samples++;
        _average = (int)(_sum / _samples);
    }

    void OnGUI()
    {
        var text = IsValid()
            ? $"Batches: {_current}\nAvg: {_average}  Min: {_min}  Max: {_max}"
            : "Batches unavailable";

        GuiColorManager.New(Color.black);
        GUI.Label(_rect.Move( -2, -2 ), text, Style );
        GuiColorManager.New(_color);
        GUI.Label(_rect, text, Style);
        GuiColorManager.Revert(2);
    }
}
