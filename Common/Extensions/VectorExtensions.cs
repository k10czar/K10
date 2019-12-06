using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 OnLineSegment( this Vector3 point, Vector3 from, Vector3 to )
	{
		var delta = to - from;
		var mag = delta.magnitude;
		var t = point - from;
		var nDir = delta / mag;
		var dot = Vector3.Dot( nDir, t );
		return from + nDir * Mathf.Clamp( dot, 0, mag );
	}
}