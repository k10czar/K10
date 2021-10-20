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
}
