using System;
using System.Text.RegularExpressions;
using K10.DebugSystem;
using UnityEngine;

[Serializable]
public class LogTrigger : ITriggerable
{
    [SerializeField,TextArea(3,10)] public string message;
    [SerializeField] public bool verbose = false;
    [SerializeField] public LogSeverity severity = LogSeverity.Info;
    [SerializeReference,ExtendedDrawer] public DebugCategory category = K10Log<TempDebug>.Category;

    public void Trigger()
    {
        Debug.Log( message );
        var log = message;

        var notError = ( severity != LogSeverity.Error );

        if (!K10DebugSystem.CanDebug( category.GetType(), verbose ? EDebugType.Verbose : EDebugType.Default) && notError) return;

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(category.Name))
        {
            var color = category.Color;
            if (verbose) color = color.AddSaturation(-0.22f);
            log = $"<b><color={color.ToHexRGB()}>[{category.Name}]</color></b> {log}";
        }

        log = K10Log.ReplaceColorsNames(log);
#else
        log = $"{(verbose?"*":"")}[{category.Name}] {Regex.Replace(log, "<.*?>", string.Empty)}";
#endif

        if (severity == LogSeverity.Error) Debug.LogError(log);
        else if (severity == LogSeverity.Warning) Debug.LogWarning(log);
        else Debug.Log(log);
    }
}

[Serializable]
public class LogTrigger<T> : ITriggerable<T>
{
    [SerializeField,TextArea(3,10)] public string message;
    [SerializeField] public bool verbose = false;
    [SerializeField] public LogSeverity severity = LogSeverity.Info;
    [SerializeReference,ExtendedDrawer] public DebugCategory category = K10Log<TempDebug>.Category;

    public void Trigger( T t )
    {
        var log = message;

        var notError = ( severity != LogSeverity.Error );

        if (!K10DebugSystem.CanDebug(category.GetType(), verbose) && notError) return;

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(category.Name))
        {
            var color = category.Color;
            if (verbose) color = color.AddSaturation(-0.22f);
            log = $"<b><color={color.ToHexRGB()}>[{category.Name}]</color></b> {log}";
        }

        log = K10Log.ReplaceColorsNames(log);
#else
        log = $"{(verbose?"*":"")}[{category.Name}] {Regex.Replace(log, "<.*?>", string.Empty)}";
#endif

        if (severity == LogSeverity.Error) Debug.LogErrorFormat(log, t.ToStringOrNull() );
        else if (severity == LogSeverity.Warning) Debug.LogWarningFormat(log, t.ToStringOrNull() );
        else Debug.LogFormat(log, t.ToStringOrNull() );
    }
}