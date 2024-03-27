using UnityEngine;
using UnityEditor;

public static class ScrollUtils
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
