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

	public static Ray RandomDeviation( this Ray ray, float maxDeviation )
	{
		float angleDeviation = K10Random.Value * ( maxDeviation * .5f );
		float randomAngle = K10Random.Value * 360;

		var a = Mathf.Deg2Rad * randomAngle;
		var cos = Mathf.Abs( Mathf.Cos( a ) * 1 );
		var sen = Mathf.Abs( Mathf.Sin( a ) * .2f );
		var prop = cos + sen;

		var dev = Quaternion.AngleAxis( angleDeviation * prop, Vector3.up );
		var rnd = Quaternion.AngleAxis( randomAngle, Vector3.forward );
		var dirRot = Quaternion.LookRotation( ray.direction, Vector3.up );

		var endDir = dirRot * rnd * dev * Vector3.forward;

		return new Ray( ray.origin, endDir );
	}
}