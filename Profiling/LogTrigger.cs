using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class LogTrigger : ITriggerable
{
    [SerializeField,TextArea(3,10)] public string message;
    [SerializeField] public bool verbose = false;
    [SerializeField] public LogSeverity severity = LogSeverity.Info;
    [SerializeReference,ExtendedDrawer] public IK10LogCategory category = K10Log<TempLogCategory>.Category;

    public void Trigger()
    {
        Debug.Log( message );
        var log = message;

        var notError = ( severity != LogSeverity.Error );
#if UNITY_EDITOR
        if (!K10DebugSystem.CanDebug( category.Name, verbose) && notError) return;
#endif

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
