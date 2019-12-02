using UnityEngine;

public static class ColliderExtensions
{
	public static Vector3 SafeClosestPoint( this CharacterController target, Vector3 point )
	{
		target.GetCapsulePoints( out Vector3 bot, out Vector3 top, out float radius );
		var corePoint = point.OnLineSegment( bot, top );
		var coreDist = ( point - corePoint );
		var coreMag = coreDist.magnitude;
		if( coreMag > radius ) coreDist = coreDist * ( radius / coreMag );
		return corePoint + coreDist;
	}

	public static void GetCapsulePoints( this CharacterController target, out Vector3 bot, out Vector3 top, out float radius )
	{
		var pos = target.transform.position;
		var center = pos + target.center;
		var scl = target.transform.lossyScale;
		radius = target.radius * Mathf.Max( scl.x, scl.z );
		var halfHeight = Mathf.Max( target.height * scl.y * .5f - radius, 0 );
		bot = center + Vector3.down * halfHeight;
		top = center + Vector3.up * halfHeight;
	}
}