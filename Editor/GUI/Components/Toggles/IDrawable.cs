using UnityEngine;

public interface IDrawable
{
    float GetHeight();
    void Draw( Rect area, GUIStyle style = null );
}

public static class DrawableExtensions
{
    public static void DrawOnTop( this IDrawable drawable, ref Rect rect, GUIStyle style = null )
    {
		var h = drawable.GetHeight();
		var area = rect.GetLineTop( h );
		drawable.Draw( area, style );
    }
}
