using UnityEngine;

public static class DefaultTextures
{
	static Texture2D _horizontalBar;

	public static Texture2D HorizontalBar
	{
		get
		{
			if (_horizontalBar == null)
			{
				var H = 16;
				_horizontalBar = new Texture2D(1, H);
				for (int x = 0; x < _horizontalBar.width; x++)
				{
					for (int y = 0; y < H; y++)
					{
						var power = Mathf.Clamp01(((float)y) / (H - 1));
						var color = Color.LerpUnclamped(Color.grey, Color.white, power);
						_horizontalBar.SetPixel(x, y, color);
					}
				}
				_horizontalBar.Apply();
			}
			return _horizontalBar;
		}
	}
	
	static Texture2D _white;

	public static Texture2D White
    {
		get
        {
			if (_white == null)
			{
				_white = new Texture2D(1, 1);
				_white.SetPixel(0, 0, Color.white);
				_white.Apply();
			}
			return _white;
        }
    }
}
