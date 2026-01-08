using System;
using System.Runtime.CompilerServices;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class Skope
    {
        #region Open Interface

        public static ILayoutScope Open(SkopeInfo info) => info.scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(info),
            EScopeType.Foldout => FoldoutScope.Open(info),
            // EScopeType.InlineHeader => expr,
            EScopeType.Inline => InlineScope.Open(info),
            _ => throw new ArgumentOutOfRangeException()
        };

        public static ILayoutScope Open(ref bool isExpandedRef, SkopeInfo info) => info.scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(ref isExpandedRef, info),
            EScopeType.Foldout => FoldoutScope.Open(ref isExpandedRef, info),
            // EScopeType.InlineHeader => expr,
            EScopeType.Inline => InlineScope.Open(ref isExpandedRef, info),
            _ => throw new ArgumentOutOfRangeException()
        };

        public static ILayoutScope Open(ref Rect rect, SkopeInfo info) => info.scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(ref rect, info),
            EScopeType.Foldout => FoldoutScope.Open(ref rect, info),
            // EScopeType.InlineHeader => expr,
            EScopeType.Inline => InlineScope.Open(ref rect, info),
            _ => throw new ArgumentOutOfRangeException()
        };

        public static ILayoutScope Open(ref Rect rect, ref bool isExpandedRef, SkopeInfo info) => info.scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(ref rect, ref isExpandedRef, info),
            EScopeType.Foldout => FoldoutScope.Open(ref rect, ref isExpandedRef, info),
            // EScopeType.InlineHeader => expr,
            EScopeType.Inline => InlineScope.Open(ref rect, ref isExpandedRef, info),
            _ => throw new ArgumentOutOfRangeException()
        };

        public static ILayoutScope Open(EScopeType scopeType, SerializedProperty property, string title) => scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(property, title),
            EScopeType.Foldout => FoldoutScope.Open(property, title),
            EScopeType.InlineHeader => InlineHeaderScope.Open(property, title),
            _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
        };

        public static ILayoutScope Header(SerializedProperty property, string title)
        {
            var info = new SkopeInfo(EScopeType.Header, property, title, EColor.Primary, EElementSize.Primary);
            return HeaderScope.Open(info);
        }

        public static ILayoutScope Open(EScopeType scopeType, string title, ref bool isExpandedRef, EColor color, EElementSize size) => scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(title, ref isExpandedRef, color, size),
            EScopeType.Foldout => FoldoutScope.Open(title, ref isExpandedRef, color, size),
            EScopeType.InlineHeader => InlineHeaderScope.Open(title, ref isExpandedRef, color, size),
            _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
        };

        public static ILayoutScope Open(EScopeType scopeType, ref Rect rect, SerializedProperty property, EColor color, EElementSize size)
            => Open(scopeType, ref rect, property, property.PrettyName(), color, size);

        public static ILayoutScope Open(EScopeType scopeType, ref Rect rect, SerializedProperty property, string title, EColor color, EElementSize size) => scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(ref rect, property, title, color, size),
            EScopeType.Foldout => FoldoutScope.Open(ref rect, property, title, color, size),
            EScopeType.InlineHeader => InlineHeaderScope.Open(ref rect, property, title, color, size),
            _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
        };

        public static ILayoutScope Open(EScopeType scopeType, ref Rect rect, string title, ref bool isExpandedRef, EColor color, EElementSize size) => scopeType switch
        {
            EScopeType.Header => HeaderScope.Open(ref rect, title, ref isExpandedRef, color, size),
            EScopeType.Foldout => FoldoutScope.Open(ref rect, title, ref isExpandedRef, color, size),
            EScopeType.InlineHeader => InlineHeaderScope.Open(ref rect, title, ref isExpandedRef, color, size),
            _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
        };

        #endregion

        #region Styles

        #region Header Styles

        private static readonly GUIStyle[] headerBoxStyles =
        {
            new("ScriptText"), // Primary
            new("HelpBox"), // Secondary
            new("SelectionRect"), // Info
            new("HelpBox"), // Success
            new("HelpBox"), // Warning
            new("HelpBox"), // Danger
            new("TE BoxBackground"), // Support
            new("HelpBox"), // Special
            new("HelpBox"), // Disabled
            new(GUIStyle.none), // Clear
            new("Wizard Box"), // Backdrop
        };

        private static readonly Color[] headerColors =
        {
            Color.white,
            Colors.Console.Dark,
            Color.white,
            Colors.Pistachio,
            Colors.Yellow, // Warning
            Colors.LightSalmon, // Danger
            Color.white, // Support
            Colors.Console.Special, // Special
            Colors.Console.Dark, // Disabled
            Color.clear, // Clear,
            Color.white, // Backdrop
        };

        #endregion

        #region Foldout Styles

        private static readonly GUIStyle[] foldoutBoxStyles = { new("HelpBox"), };

        private static readonly Color[] foldoutColors =
        {
            Color.white, // Primary
            Colors.Console.Dark, // Secondary
            Colors.Console.Secondary, // Info
            Colors.Pistachio, // Success
            Colors.Yellow, // Warning
            Colors.LightSalmon, // Danger
            Color.white, // Support
            Colors.Console.Special, // Special
            Colors.Console.Dark, // Disabled
            Color.clear, // Clear,
            Color.white, // Backdrop
        };

        #endregion

        #region Inline Styles

        private static readonly GUIStyle[] inlineBoxStyles = { SkyxStyles.WhiteBackgroundStyle };

        private static readonly Color[] inlineColors =
        {
            Color.white,
            Colors.Console.Dark,
            Color.white,
            Colors.Pistachio,
            Colors.Yellow, // Warning
            Colors.LightSalmon, // Danger
            Color.white, // Support
            Colors.Console.Special, // Special
            Colors.Console.Dark, // Disabled
            Color.clear, // Clear,
            Color.white, // Backdrop
        };

        #endregion

        private static readonly float[] headerHeights =
        {
            32, // Primary
            28, // Secondary
            24, // SingleLine
        };

        private static readonly GUIStyle[][] boxStyles =
        {
            headerBoxStyles,
            foldoutBoxStyles,
            headerBoxStyles,
            inlineBoxStyles,
        };

        private static readonly Color[][] boxColors =
        {
            headerColors,
            foldoutColors,
            headerColors,
            inlineColors,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float HeaderHeight(EElementSize size) => headerHeights.GetClamped(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ScopeHeight(SkopeInfo info, bool isExpanded)
            => ScopeHeight(info.scopeType, info.size, isExpanded);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ScopeHeight(EScopeType scopeType, bool isExpanded)
            => ScopeHeight(scopeType, scopeType.PreferredSize(), isExpanded);

        public static float ScopeHeight(EScopeType scopeType, EElementSize size, bool isExpanded)
        {
            var baseHeight = HeaderHeight(size);

            var margin = !isExpanded ? 1 : scopeType switch
            {
                EScopeType.Header => 3,
                EScopeType.Foldout => 3,
                EScopeType.InlineHeader => 2,
                EScopeType.Inline => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
            };

            baseHeight += margin * SkyxStyles.ElementsMargin;

            return baseHeight;
        }

        public static void DrawBox(ref Rect rect, SkopeInfo info)
        {
            var boxStyle = boxStyles[(int)info.scopeType].GetClamped(info.color);
            var boxColor = boxColors[(int)info.scopeType].GetClamped(info.color);

            using var scope = BackgroundColorScope.Set(boxColor);
            GUI.Box(rect, GUIContent.none, boxStyle);
        }

        #endregion
    }
}