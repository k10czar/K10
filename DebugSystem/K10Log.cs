using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace K10.DebugSystem
{
    public static class K10Log<T> where T : IDebugCategory, new()
    {
        private static readonly T category = K10DebugSystem.GetCategory<T>();

        public static string Name => category.Name;
        public static T Category => category;
        public static Color Color => category.Color;
        public static Color SecondaryColor => category.SecondaryColor;

        private static bool ShouldAlwaysDebug(LogSeverity severity)
            => severity is LogSeverity.Error || typeof(T) == typeof(TempDebugCategory);

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log(LogSeverity severity, string log, bool verbose, Object consoleTarget, IEnumerable<Object> owners)
        {
            if (!ShouldAlwaysDebug(severity) && (!K10DebugSystem.CanDebug<T>(verbose) || !K10DebugSystem.CheckDebugOwners(owners)))
                return;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(category.Name))
            {
                var color = category.Color;
                if (verbose) color = color.AddSaturation(-0.22f);
                log = $"<b><color={color.ToHexRGB()}>[{category.Name}]</color></b> {log}\nOwners: {string.Join(", ", owners)}";
            }

            log = K10Log.ReplaceColorsNames(log);
#else
            log = $"[{category.Name}] {Regex.Replace(log, "<.*?>", string.Empty)}";
#endif

            if (severity == LogSeverity.Error) Debug.LogError(log, consoleTarget);
            else if (severity == LogSeverity.Warning) Debug.LogWarning(log, consoleTarget);
            else Debug.Log(log, consoleTarget);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log(LogSeverity severity, string log) => Log(severity, log, severity is LogSeverity.Warning, null, Array.Empty<Object>());

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log(string log, Object consoleTarget = null) => Log(LogSeverity.Info, log, false, consoleTarget, new[] { consoleTarget });

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose(string log, Object consoleTarget = null) => Log(LogSeverity.Warning, log, true, consoleTarget, new[] { consoleTarget });
    }

    public static class K10Log
    {
        public const string ConditionalDirective = "ENABLE_K10LOG";

        [LazyConst] private static Dictionary<string, Color> EDITOR_colorReplaceDict = null;

        private static Dictionary<string, Color> EDITOR_ColorReplace
        {
            get
            {
                if (EDITOR_colorReplaceDict == null)
                {
                    EDITOR_colorReplaceDict = new();

                    foreach (var color in Colors.All) EDITOR_colorReplaceDict.Add(color.Key.ToLower(), color.Value);
                    foreach (var color in Colors.Console.All) EDITOR_colorReplaceDict.Add(color.Key.ToLower(), color.Value);
                }

                return EDITOR_colorReplaceDict;
            }
        }

        public static string ReplaceColorsNames(string log)
        {
            var colorTagStart = "<color=";
            var len = log.Length;
            var tLen = colorTagStart.Length;
            var lti = len - tLen;
            int i = 0;
            int j = 0;

            StringBuilder sb = null;
            int rescribed = 0;

            for (; i < lti && (i + j) < len; j++)
            {
                var letter = log[i + j];
                if (j >= tLen)
                {
                    if (letter == '>')
                    {
                        var firstLetter = log[i + tLen];
                        if (firstLetter != '#')
                        {
                            var k = j - tLen;
                            var colorName = log.Substring(i + tLen, k);
                            var lowerName = colorName.ToLower();
                            // Debug.Log( $"Found color:{colorName} i:{i} j:{j} tLen:{tLen} k:{k} lti:{lti} len:{len} {EDITOR_ColorReplace.TryGetValue( lowerName, out var color22 )} on {log}" );
                            // Debug.Log( $"{log}\n{log.Substring(0,i)}#{colorName}#{log.Substring(i+j+1)}" );
                            if (EDITOR_ColorReplace.TryGetValue(lowerName, out var color))
                            {

                                if (sb == null) sb = ObjectPool<StringBuilder>.Request();
                                sb.Append(log.Substring(rescribed, i + tLen - rescribed));

                                var colorCode = ColorUtility.ToHtmlStringRGB(color);
                                sb.Append($"#{colorCode}>");

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

                if (letter == colorTagStart[j]) continue;
                i++;
                j = 0;
            }

            if (sb != null)
            {
                if (rescribed < len) sb.Append(log.Substring(rescribed));
                log = sb.ToString();
                ObjectPool<StringBuilder>.Return(sb);
            }

            return log;
        }
    }
}