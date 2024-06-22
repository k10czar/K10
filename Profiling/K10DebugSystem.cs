using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

#if UNITY_EDITOR
public static class K10DebugSystem
{
    public static bool CanDebug<T>( bool verbose = false ) where T : IK10LogCategory, new()
            => EditorPrefs.GetBool( GetSaveKey<T>() ) || typeof(T) == typeof(TempLogCategory);

    private static string GetSaveKey<T>( bool verbose = false ) where T : IK10LogCategory, new()
            => $"{K10Log<T>.Name}{(verbose?"_VERBOSE":"")}_LOG_ENABLED";

    public static void ToggleLog<T>() where T : IK10LogCategory, new()
    {
        var key = GetSaveKey<T>();
        var isEnabled = !EditorPrefs.GetBool(key);
        EditorPrefs.SetBool(key, isEnabled);
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

    public static EDebugTargets DebugTargets() => (EDebugTargets) EditorPrefs.GetInt(DEBUG_TARGETS_SAVE_KEY);
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

    public static bool CanDebugTarget(MonoBehaviour targetBehaviour, LogSeverity severity)
    {
        if (DebugErrors() && severity is LogSeverity.Danger) return true;

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
    #endregion

    static K10DebugSystem()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
}
#else
public static class K10Debug
{
    public static Dictionary<GameSystem, Action<bool>> gameSystemObservers = new();

    public static bool CanDebug(GameSystem systemId)
    {
        var id = (int)systemId;
        var mod = id % 3;
        return id > 0 && mod == 0;
    }

    public static bool IsHidingVirtualTargets => true;
    public static bool IsHidingInGameUIs => true;
}
#endif