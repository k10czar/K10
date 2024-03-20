using UnityEngine;

public static class TextureExtensions
{
    public static void DrawLeft( this Texture texture, ref Rect rect, float spacing = 0 )
    {
        var maxH = Mathf.Min( texture.height, rect.height );
        var maxW = Mathf.Min( texture.width, rect.width - spacing );
        var maxScale = Mathf.Min( maxW / texture.width, maxH / texture.height );
        var usedW = texture.width * maxScale;
        var drawRect = rect.RequestLeft( usedW );
        drawRect = drawRect.RequestHeight( texture.height * maxScale );
        GUI.Label( drawRect, texture );
        rect = rect.CutLeft( usedW + spacing );
    }
}
