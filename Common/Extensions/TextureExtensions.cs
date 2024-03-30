using UnityEngine;

public static class TextureExtensions
{
    public static void DrawLeft( this Texture texture, ref Rect rect, float spacing = 0 )
    {
        if( texture == null ) return;
        var maxH = Mathf.Min( texture.height, rect.height );
        var maxW = Mathf.Min( texture.width, rect.width - spacing );
        var maxScale = Mathf.Min( maxW / texture.width, maxH / texture.height );
        var usedW = texture.width * maxScale;
        var drawRect = rect.RequestLeft( usedW );
        drawRect = drawRect.RequestHeight( texture.height * maxScale );
        GUI.Label( drawRect, texture );
        rect = rect.CutLeft( usedW + spacing );
    }

    public static void DrawLayoutWithHeight( this Texture texture, float height )
    {
        if( texture == null ) return;
        var maxScale = height / texture.height;
        GUILayout.Label( texture, GUILayout.Width( texture.width * maxScale ), GUILayout.Height( height ) );
    }

    public static void DrawLayoutWithWidth( this Texture texture, float width )
    {
        if( texture == null ) return;
        var maxScale = width / texture.width;
        GUILayout.Label( texture, GUILayout.Width( width ), GUILayout.Height( texture.height * maxScale ) );
    }
}
