using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

#if UNITY_EDITOR
public static class K10DebugSystem
{
    public static bool CanDebug<T>(bool verbose = false) where T : IK10LogCategory, new()
        => CanDebug(K10Log<T>.Name, verbose) || typeof(T) == typeof(TempLogCategory);

    public static bool CanDebugVisuals<T>() where T : IK10LogCategory, new()
        => CanDebugVisuals(K10Log<T>.Name) || typeof(T) == typeof(TempLogCategory);

    public static bool SkipVisuals<T>() where T : IK10LogCategory, new()
        => !CanDebugVisuals<T>();

    public static bool CanDebugVisuals(string baseName)
        => EditorPrefs.GetBool(GetVisualsSaveKey(baseName));

    public static bool CanDebug(string baseName, bool verbose = false)
        => EditorPrefs.GetBool(GetSaveKey(baseName, verbose));

    private static string GetSaveKey<T>(bool verbose = false) where T : IK10LogCategory, new()
        => GetSaveKey(K10Log<T>.Name, verbose);

    private static string GetSaveKey(string baseName, bool verbose = false) => $"{baseName}{(verbose ? "_VERBOSE" : "")}_LOG_ENABLED";
    private static string GetVisualsSaveKey(string baseName) => $"{baseName}_VISUALS_DEBUG_ENABLED";

    public static void ToggleLog(string baseName, bool verbose = false)
    {
        var key = GetSaveKey(baseName, verbose);
        ToggleLogWithKey(key);
    }

    public static void ToggleVisualsLog(string baseName) => ToggleLogWithKey(GetVisualsSaveKey(baseName));

    public static void SetLog(string baseName, bool value, bool verbose = false)
    {
        var key = GetSaveKey(baseName, verbose);
        SetLogWithKey(key, value);
    }

    public static bool GetLog(string baseName, bool verbose = false)
    {
        var key = GetSaveKey(baseName, verbose);
        return EditorPrefs.GetBool(key);
    }

    public static bool GetVisualsLog(string baseName)
    {
        var key = GetVisualsSaveKey(baseName);
        return EditorPrefs.GetBool(key);
    }

    public static void SetVisualsLog(string baseName, bool value) => SetLogWithKey(GetVisualsSaveKey(baseName), value);

    private static void ToggleLogWithKey(string key)
    {
        var isEnabled = !EditorPrefs.GetBool(key);
        SetLogWithKey(key, isEnabled);
    }

    private static void SetLogWithKey(string key, bool value)
    {
        EditorPrefs.SetBool(key, value);
    }

    #region Debug Targets

    public enum EDebugTargets
    {
        Disabled,
        All,
        OnlySelected,
        NullAndSelected,
    }

    private const string DEBUG_TARGETS_SAVE_KEY = "DEBUG_TARGETS_SAVE_KEY";
    private const string DEBUG_ERRORS_SAVE_KEY = "DEBUG_ERRORS_SAVE_KEY";

    public static EDebugTargets DebugTargets() => (EDebugTargets)EditorPrefs.GetInt(DEBUG_TARGETS_SAVE_KEY);

    public static void ToggleDebugTargets()
    {
        var current = EditorPrefs.GetInt(DEBUG_TARGETS_SAVE_KEY);
        current = (current + 1) % Enum.GetValues(typeof(EDebugTargets)).Length;
        EditorPrefs.SetInt(DEBUG_TARGETS_SAVE_KEY, current);
    }

    public static bool DebugErrors() => EditorPrefs.GetBool(DEBUG_ERRORS_SAVE_KEY);

    public static void ToggleDebugErrors()
    {
        EditorPrefs.SetBool(DEBUG_ERRORS_SAVE_KEY, !EditorPrefs.GetBool(DEBUG_ERRORS_SAVE_KEY));
    }

    public static bool CanDebugTarget(Component targetBehaviour, LogSeverity severity = LogSeverity.Info)
    {
        if (DebugErrors() && severity is LogSeverity.Error) return true;

        var target = targetBehaviour == null ? null : targetBehaviour.gameObject;

        return DebugTargets() switch
        {
            EDebugTargets.Disabled => false,
            EDebugTargets.All => true,
            EDebugTargets.OnlySelected => selectedTargets.Contains(target),
            EDebugTargets.NullAndSelected => target == null || selectedTargets.Contains(target),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static readonly List<GameObject> selectedTargets = new();

    #endregion

    #region Observers

    private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        selectedTargets.Clear();
    }

    public static void AddTarget(GameObject go)
    {
        selectedTargets.Add(go);
    }

    public static void RemoveTarget(GameObject go)
    {
        selectedTargets.Remove(go);
    }

    #endregion

    static K10DebugSystem()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
}
#else
public static class K10DebugSystem
{
    public static bool CanDebug<T>(bool verbose = false) where T : IK10LogCategory, new()
        => false;

    public static bool CanDebugVisuals<T>() where T : IK10LogCategory, new()
        => false;

    public static bool SkipVisuals<T>() where T : IK10LogCategory, new()
        => true;

    public static bool CanDebugVisuals(string baseName)
        => false;

    public static bool CanDebug(string baseName, bool verbose = false)
        => false;
}
#endif