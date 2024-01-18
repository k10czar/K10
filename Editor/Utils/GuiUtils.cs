using UnityEngine;
using UnityEditor;

public static class GuiUtils
{
    public static class Scroll
    {
        public static void DrawFakeLines( int lines )
        {
            if( lines <= 0 ) return;
            var slh = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            GUILayout.Label( GUIContent.none, GUILayout.Height( slh * lines ) );
        }

        public static void CalculateLinesToDraw( Vector2 scroll, float height, out int startId, out int lines )
        {
            var slh = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            startId = Mathf.FloorToInt( scroll.y / slh );
            lines = Mathf.RoundToInt( height / slh ) + 1;
        }
    }

    public static class Label
    {
        public static void ExactSizeLayout( string label )
        {
            var content = new GUIContent( label );
            GUILayout.Label( content, GUILayout.Width( GUI.skin.label.CalcSize( content ).x ) );
        }

        public static void ExactSizeLayout( string label, Color color )
        {
            GuiColorManager.New( color );
            ExactSizeLayout( label );
            GuiColorManager.Revert();
        }
    }

    public static class Toggle
    {
        public static bool ExactSizeLayout( ref bool value, string label )
        {
            var content = new GUIContent( label );
            var newValue = GUILayout.Toggle( value, content, GUILayout.Width( GUI.skin.label.CalcSize( content ).x + 16 ) );
            var changed = value != newValue;
            value = newValue;
            return changed;
        }
    }
}
