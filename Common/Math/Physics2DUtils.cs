using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace K10
{
	public static class Physics2DUtils
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GetProjectedRadiusMaxSize( Camera cam, float3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Max( up, Mathf.Max( down, Mathf.Max( right, left ) ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GetProjectedRadiusAverageSize( Camera cam, float3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return ( up + down + right + left ) * .25f;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GetProjectedRadiusMinSize( Camera cam, float3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Min( up, Mathf.Min( down, Mathf.Min( right, left ) ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void GetProjectedRadiusSizes( Camera cam, float3 worldPos, float radius, out float right, out float up, out float left, out float down )
		{
			var cwUp = math.cross( cam.transform.forward, cam.transform.up ) * radius;
			var cwDown = math.cross( cam.transform.forward, -cam.transform.up ) * radius;
			var cwRight = math.cross( cam.transform.forward, cam.transform.right ) * radius;
			var cwLeft = math.cross( cam.transform.forward, -cam.transform.right ) * radius;

			float2 center = cam.WorldToScreenPoint( worldPos ).IgnoreZ();

			float2 cUp = cam.WorldToScreenPoint( worldPos + cwUp ).IgnoreZ();
			float2 cDown = cam.WorldToScreenPoint( worldPos + cwDown ).IgnoreZ();
			float2 cRight = cam.WorldToScreenPoint( worldPos + cwRight ).IgnoreZ();
			float2 cLeft = cam.WorldToScreenPoint( worldPos + cwLeft ).IgnoreZ();

			up = math.length( cUp - center );
			down = math.length( cDown - center );
			right = math.length( cRight - center );
			left = math.length( cLeft - center );
		}

		// static float2 Test( Camera cam, Matrix4x4 mat, float3 pos )
		// {
		// 	var temp = ( mat * new float4( pos.x, pos.y, pos.z, 1 ) );
		// 	temp.x = ( temp.x / temp.w + 1f ) * .5f * cam.pixelWidth;
		// 	temp.y = ( temp.y / temp.w + 1f ) * .5f * cam.pixelHeight;
		// 	return temp;
		// }

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Collinear( float2 p, float2 q, float2 r ) { return Aprox( Cross( q - p, r - p ), 0 ); }
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float Cross( float2 a, float2 b ) { return a.x * b.y - a.y * b.x; }
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool CCW( float2 p, float2 q, float2 r ) { return Cross( q - p, r - p ) > 0; }
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Aprox( float a, float b ) => math.abs( a - b ) < 2 * float.Epsilon;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float Angle( float2 a, float2 o, float2 b )
		{
			var oa = a - o;
			var ob = b - o;
			return math.acos( math.dot( oa, ob ) / math.sqrt( math.lengthsq( oa ) * math.lengthsq( ob ) ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( float2 p, Ray2D ray, out float2 hitPoint )
		{
			var b = ray.origin + ray.direction.normalized;
			var a = ray.origin;
			return DistanceToLine( p, a, b, out hitPoint );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( float2 p, float2 a, float2 b, out float2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( float2 p, float2 a, float2 b, out float2 hitPoint, float projA, float projB )
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var scale = projA / ( dx * dx + dy * dy );
			hitPoint = a + scale * ( b - a );
			return math.length( p - hitPoint );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToPoint( float2 p, Ray2D ray, out float2 hitPoint, float range = float.MaxValue )
		{
			var b = ray.origin + ray.direction.normalized * range;
			var a = ray.origin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToPoint( float2 p, float2 rayOrigin, float2 rayDir, out float2 hitPoint, float range = float.MaxValue )
		{
			var b = rayOrigin + rayDir * range;
			var a = rayOrigin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToPoint( float2 p, float2 a, float2 b, out float2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			if( Aprox( projA, 0 ) || projA < 0 )
			{
				hitPoint = a;
				return math.length( p - a );
			}

			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			if( Aprox( projB, 0 ) || projB < 0 )
			{
				hitPoint = b;
				return math.length( p - b );
			}

			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool AreParellel( float2 l1a, float2 l1b, float2 l2a, float2 l2b )
		{
			var l1Dir = math.normalize( l1b - l1b );
			var l2Dir = math.normalize( l2b - l2b );
			return Aprox( l1Dir.x, l2Dir.x ) && Aprox( l1Dir.y, l2Dir.y );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float FindScale( float2 l1a, float2 l1b )
		{
			if( Aprox( l1a.x, 0 ) ) return l1b.y / l1a.y;
			return l1b.x / l1a.x;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Intersects( float2 l1a, float2 l1b, float2 l2a, float2 l2b, out float2 hitPoint )
		{
			hitPoint = l1a;

			float A1 = l1b.y - l1a.y;
			float B1 = l1a.x - l1b.x;
			float C1 = A1 * l1a.x + B1 * l1a.y;

			float A2 = l2b.y - l2a.y;
			float B2 = l2a.x - l2b.x;
			float C2 = A2 * l2a.x + B2 * l2a.y;

			float delta = A1 * B2 - A2 * B1;
			if( Aprox( delta, 0 ) ) return false;

			hitPoint.x = ( B2 * C1 - B1 * C2 ) / delta;
			hitPoint.y = ( A1 * C2 - A2 * C1 ) / delta;
			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Ray ray, float3 point, out float3 projectedPoint )
		{
			var t = point - ( (float3)ray.origin );
			var dl = math.length( math.cross( ray.direction, t ) );
			var dot = math.dot( ray.direction, t );
			projectedPoint = ray.origin + ray.direction * dot;
			return dl;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Ray ray, float3 point )
		{
			return math.length( math.cross( ray.direction, point - ( (float3)ray.origin ) ) );
		}
	}
}