using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public enum LogSeverity { Info, Warning, Error }

public interface IK10LogCategory
{
    string Name { get; }
    Color Color { get; }
}

public class TempLogCategory : IK10LogCategory
{
    public string Name => "Temp";
#if UNITY_EDITOR
    public Color Color => Colors.Orange;
#endif
}

public static class K10Log<T> where T : IK10LogCategory, new()
{
    static readonly T category = new T();

    public static string Name => category.Name;
    public static T Category => category;

    public static bool Can(bool verbose = false) => K10DebugSystem.CanDebug<T>();
    public static bool Skip(bool verbose = false) => !K10DebugSystem.CanDebug<T>();
    public static bool SkipVisuals() => K10DebugSystem.SkipVisuals<T>();

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log(string log, LogSeverity severity = LogSeverity.Info, MonoBehaviour target = null, bool verbose = false)
        => Log(severity, log, target, verbose);

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose(string log, MonoBehaviour target = null, LogSeverity severity = LogSeverity.Warning)
        => Log(severity, log, target, true);

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogException(System.Exception exception, MonoBehaviour target = null)
    {
        Debug.LogException(exception, target);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log(LogSeverity severity, string log, MonoBehaviour target = null, bool verbose = false)
    {
        var notError = ( severity != LogSeverity.Error );
#if UNITY_EDITOR
        if (!K10DebugSystem.CanDebug<T>(verbose) && notError) return;
        if (!K10DebugSystem.CanDebugTarget(target, severity) && notError) return;
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

        if (severity == LogSeverity.Error) Debug.LogError(log, target);
        else if (severity == LogSeverity.Warning) Debug.LogWarning(log, target);
        else Debug.Log(log, target);
    }

#if UNITY_EDITOR
    public static void SetGizmosColor()
    {
        GizmosColorManager.New(category.Color);
    }

    public static void RevertGizmosColor()
    {
        GizmosColorManager.Revert();
    }
#endif
}

public static class K10Log
{
    public const string ConditionalDirective = "ENABLE_K10LOG";
    [LazyConst] private static Dictionary<string,Color> EDITOR_colorReplaceDict = null;
    private static Dictionary<string,Color> EDITOR_ColorReplace
    {
        get
        {
            if( EDITOR_colorReplaceDict == null )
            {
                EDITOR_colorReplaceDict = new();

                foreach( var color in Colors.All ) EDITOR_colorReplaceDict.Add( color.Key.ToLower(), color.Value );
                foreach( var color in Colors.Console.All ) EDITOR_colorReplaceDict.Add( color.Key.ToLower(), color.Value );
            }
            return EDITOR_colorReplaceDict;
        }
    }

    public static string ReplaceColorsNames( string log )
    {
        var colorTagStart = "<color=";
        var len = log.Length;
        var tLen = colorTagStart.Length;
        var lti = len - tLen;
        int i = 0;
        int j = 0;

        StringBuilder sb = null;
        int rescribed = 0;

        for( ; i < lti && (i+j) < len; j++ )
        {
            var letter = log[i+j];
            if( j >= tLen )
            {
                if( letter == '>' )
                {
                    var firstLetter = log[i + tLen];
                    if( firstLetter != '#' )
                    {
                        var k = j - tLen;
                        var colorName = log.Substring( i + tLen, k );
                        var lowerName = colorName.ToLower();
                        // Debug.Log( $"Found color:{colorName} i:{i} j:{j} tLen:{tLen} k:{k} lti:{lti} len:{len} {EDITOR_ColorReplace.TryGetValue( lowerName, out var color22 )} on {log}" );
                        // Debug.Log( $"{log}\n{log.Substring(0,i)}#{colorName}#{log.Substring(i+j+1)}" );
                        if( EDITOR_ColorReplace.TryGetValue( lowerName, out var color ) )
                        {

                            if( sb == null ) sb = ObjectPool<StringBuilder>.Request();
                            sb.Append( log.Substring( rescribed, i + tLen - rescribed ) );

                            var colorCode = ColorUtility.ToHtmlStringRGB(color);
                            sb.Append( $"#{colorCode}>" );

                            i += tLen + colorName.Length + 1;
                            rescribed = i;

                            j = -1;
                            continue;
                        }
                    }
                    i = i + j + 1;
                    j = -1;
                }
                continue;
            }
            if( letter == colorTagStart[j] ) continue;
            i++;
            j=0;
        }

        if( sb != null )
        {
            if( rescribed < len ) sb.Append( log.Substring( rescribed ) );
            log = sb.ToString();
            ObjectPool<StringBuilder>.Return( sb );
        }
        return log;
    }
}