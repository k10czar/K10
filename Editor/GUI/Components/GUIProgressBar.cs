using UnityEditor;
using UnityEngine;

public static class GUIProgressBar
{
	static Color GetDefaultColor(float val)
	{
		var r = Mathf.Clamp01(val * 2);
		// var g = Mathf.Abs( 2 * val - 1 ) * .5f + .5f;
		var g = Mathf.Clamp01(2 * val - 1);
		var b = 1 - val * 2;
		return new Color(r * .9f + .1f, g * .9f + .1f, b * .9f + .1f);
	}

	public static void Draw(Rect r, float val) => Draw(r, val, GetDefaultColor(val));
	public static void Draw(Rect r, float val, Color color)
	{
		var label = $"{(val * 100):N1}%";

		GuiColorManager.New(new Color(.75f, .75f, .75f, .75f));
		GUI.Box(r, "");
		GuiColorManager.Revert();
		if (!Mathf.Approximately(val, 0) && val > 0)
		{
			var br = r;
			br.width = r.width * val;
			GuiColorManager.New(color);
			GUI.DrawTexture(br.Shrink(2), DefaultTextures.HorizontalBar);
			GuiColorManager.Revert();
		}
		GUI.Label(r, label, K10GuiStyles.smallCenterStyle);
	}

	public static void DrawLayout(float val, params GUILayoutOption[] options) => DrawLayout(val, GetDefaultColor(val), options);
	public static void DrawLayout(float val, Color color, params GUILayoutOption[] options)
	{
		var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, options);
		Draw(rect, val, color);
	}

	public static void DrawLayout(float val, float w, float h) => DrawLayout(val, GetDefaultColor(val), w, h);
	public static void DrawLayout(float val, Color color, float w, float h)
	{
		var rect = GUILayoutUtility.GetRect(w, h);
		Draw(rect, val, color);
	}
}
