using UnityEngine;

public static class TransformExtensions
{
	public static bool Contains( this Transform parent, Transform candidate )
	{
		while( candidate != null )
		{
			if( parent == candidate ) return true;
			candidate = candidate.parent;
		}
		return false;
	}

	public static bool ResizesWithParent(this RectTransform rectTransform, RectTransform.Axis axis)
	{
		if (axis is RectTransform.Axis.Horizontal)
			return !Mathf.Approximately(rectTransform.anchorMin.x, rectTransform.anchorMax.x);

		return !Mathf.Approximately(rectTransform.anchorMin.y, rectTransform.anchorMax.y);
	}
}