
using System;
using UnityEngine;

public class BatchesStatistics : MonoBehaviour
{
    [SerializeField] Rect _rect = new Rect(10, 10, 800, 150);
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

    public SampledStatistics Batches { get; } = new SampledStatistics(new BatchesSampler());
    public SampledStatistics Fps { get; } = new SampledStatistics(new FpsSampler());

    SampledStatistics[] _tracked;
    SampledStatistics[] Tracked => _tracked ??= new[] { Batches, Fps };

    bool _counting = true;

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

    public static void DestroyInstance()
    {
        if( _instance == null )
            return;

        GameObject.Destroy( _instance.gameObject );
        _instance = null;
    }

    public void Reset()
    {
        foreach (var tracked in Tracked) tracked.Reset();
        _counting = true;
    }

    public void Stop()
    {
        _counting = false;
    }

    void OnEnable()
    {
        foreach (var tracked in Tracked) tracked.Start();
    }

    void OnDisable()
    {
        foreach (var tracked in Tracked) tracked.Stop();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	// Editor uses the exact Stats-window value; development builds read the profiler counter (stripped from
	// release builds, hence the guard).
	long GetCurrentBatches()
	{
#if UNITY_EDITOR
#if UNITY_6000_4_OR_NEWER
		return UnityEditor.UnityStats.drawCalls;   // renamed from `drawCalls` in Unity 6.4
#else
		return UnityEditor.UnityStats.batches;
#endif
#else
		return _batchesRecorder.Valid ? _batchesRecorder.LastValue : 0;
#endif // UNITY_EDITOR
	}

    void Update()
    {
        if (!_counting)
            return;

        foreach (var tracked in Tracked) tracked.Update();
    }

    void OnGUI()
    {
        var text = string.Join("\n", System.Array.ConvertAll(Tracked, t => t.ToReport()));

        GuiColorManager.New(Color.black);
        GUI.Label(_rect.Move( -2, -2 ), text, Style );
        GuiColorManager.New(_color);
        GUI.Label(_rect, text, Style);
        GuiColorManager.Revert(2);
    }
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
}
