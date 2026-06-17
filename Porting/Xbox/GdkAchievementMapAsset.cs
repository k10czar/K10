using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GdkAchievementEntry
{
    public string code;
    public string achievementId;
}

[CreateAssetMenu(
    fileName = "GdkAchievementMap",
    menuName = "Cavylabs/Platform Services/GDK Achievement Map")]
public sealed class GdkAchievementMapAsset : ScriptableObject
{
    [SerializeField] private List<GdkAchievementEntry> entries = new();

    private Dictionary<string, string> _map;

    public bool TryResolve(string code, out string achievementId)
    {
        BuildCacheIfNeeded();
        return _map.TryGetValue(code, out achievementId);
    }

    private void BuildCacheIfNeeded()
    {
        if (_map != null)
            return;

        _map = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.code))
                continue;

            if (string.IsNullOrWhiteSpace(entry.achievementId))
            {
                Debug.LogWarning($"[GDK][ACH] Empty achievement id in map: {entry.code}", this);
                continue;
            }

            if (_map.ContainsKey(entry.code))
            {
                Debug.LogWarning($"[GDK][ACH] Duplicate achievement code in map: {entry.code}", this);
                continue;
            }

            _map.Add(entry.code, entry.achievementId);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _map = null;
    }
#endif
}
