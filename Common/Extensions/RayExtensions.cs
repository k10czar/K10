using UnityEngine;


public static class RayExtensions
{
	public static Vector3 GetWithY( this Ray ray, float y )
	{
		var distance = ( ( y - ray.origin.y ) / ray.direction.y );
		return ray.origin + ( ray.direction * distance );
	}

	public static Vector3 GetWithZ( this Ray ray, float z )
	{
		var distance = ( ( z - ray.origin.z ) / ray.direction.z );
		return ray.origin + ( ray.direction * distance );
	}

	public static Vector3 GetWithYonlyFoward( this Ray ray, float y, float maxDist )
	{
		var pos = ray.origin;
		var dir = ray.direction;

		if( Mathf.Approximately( dir.y, 0 ) || !Mathf.Approximately( Mathf.Sign( dir.y ), Mathf.Sign( y - pos.y ) ) )
		{
			float xzNorm = dir.x * dir.x + dir.z * dir.z;
			xzNorm = Mathf.Sqrt( xzNorm );
			float dist = maxDist / xzNorm;
            return pos + new Vector3( dir.x * dist, 0, dir.z * dist );
		}

		var distance = ( ( y - pos.y ) / dir.y );
		return pos + ( dir * distance );
	}
}