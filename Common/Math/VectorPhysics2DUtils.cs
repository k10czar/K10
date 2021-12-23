using System.Runtime.CompilerServices;
using UnityEngine;

namespace K10
{
	public static class Physics2DUtils
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GetProjectedRadiusMaxSize( Camera cam, Vector3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Max( up, Mathf.Max( down, Mathf.Max( right, left ) ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GetProjectedRadiusAverageSize( Camera cam, Vector3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return ( up + down + right + left ) * .25f;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GetProjectedRadiusMinSize( Camera cam, Vector3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Min( up, Mathf.Min( down, Mathf.Min( right, left ) ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void GetProjectedRadiusSizes( Camera cam, Vector3 worldPos, float radius, out float right, out float up, out float left, out float down )
		{
            var cwUp = Vector3.Cross( cam.transform.forward, cam.transform.up ) * radius;
            var cwDown = Vector3.Cross( cam.transform.forward, -cam.transform.up ) * radius;
            var cwRight = Vector3.Cross( cam.transform.forward, cam.transform.right ) * radius;
            var cwLeft = Vector3.Cross( cam.transform.forward, -cam.transform.right ) * radius;

			Vector2 center = cam.WorldToScreenPoint( worldPos );

			Vector2 cUp = cam.WorldToScreenPoint( worldPos + cwUp );
			Vector2 cDown = cam.WorldToScreenPoint( worldPos + cwDown );
			Vector2 cRight = cam.WorldToScreenPoint( worldPos + cwRight );
			Vector2 cLeft = cam.WorldToScreenPoint( worldPos + cwLeft );

			up = ( cUp - center ).magnitude;
			down = ( cDown - center ).magnitude;
			right = ( cRight - center ).magnitude;
			left = ( cLeft - center ).magnitude;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static Vector2 Test( Camera cam, Matrix4x4 mat, Vector3 pos )
		{
            var temp = ( mat * new Vector4( pos.x, pos.y, pos.z, 1 ) );
            temp.x = ( temp.x / temp.w + 1f ) * .5f * cam.pixelWidth;
            temp.y = ( temp.y / temp.w + 1f ) * .5f * cam.pixelHeight;
			return temp;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Collinear( Vector2 p, Vector2 q, Vector2 r ) { return Mathf.Approximately( Cross( q - p, r - p ), 0 ); }
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float Cross( Vector2 a, Vector2 b ) { return a.x * b.y - a.y * b.x; }
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool CCW( Vector2 p, Vector2 q, Vector2 r ) { return Cross( q - p, r - p ) > 0; }

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float Angle( Vector2 a, Vector2 o, Vector2 b )
		{
			var oa = a - o;
			var ob = b - o;
			return Mathf.Acos( Vector2.Dot( oa, ob ) / Mathf.Sqrt( oa.sqrMagnitude * ob.sqrMagnitude ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Vector2 p, Ray2D ray, out Vector2 hitPoint )
		{
			var b = ray.origin + ray.direction.normalized;
			var a = ray.origin;
			return DistanceToLine( p, a, b, out hitPoint );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Vector2 p, Vector2 a, Vector2 b, out Vector2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Vector2 p, Vector2 a, Vector2 b, out Vector2 hitPoint, float projA, float projB )
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var scale = projA / ( dx * dx + dy * dy );
			hitPoint = a + scale * ( b - a );
			return ( p - hitPoint ).magnitude;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToPoint( Vector2 p, Vector2 rayOrigin, Vector2 rayDirection, out Vector2 hitPoint, float range = float.MaxValue )
		{
			var b = rayOrigin + rayDirection * range;
			var a = rayOrigin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToPoint( Vector2 p, Vector2 a, Vector2 b, out Vector2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			if( Mathf.Approximately( projA, 0 ) || projA < 0 )
			{
				hitPoint = a;
				return ( p - a ).magnitude;
			}

			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			if( Mathf.Approximately( projB, 0 ) || projB < 0 )
			{
				hitPoint = b;
				return ( p - b ).magnitude;
			}

			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool AreParellel( Vector2 l1a, Vector2 l1b, Vector2 l2a, Vector2 l2b )
		{
			var l1Dir = ( l1b - l1b ).normalized;
			var l2Dir = ( l2b - l2b ).normalized;
			return Mathf.Approximately( l1Dir.x, l2Dir.x ) && Mathf.Approximately( l1Dir.y, l2Dir.y );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float FindScale( Vector2 l1a, Vector2 l1b )
		{
			if( Mathf.Approximately( l1a.x, 0 ) ) return l1b.y / l1a.y;
			return l1b.x / l1a.x;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Intersects( Vector2 l1a, Vector2 l1b, Vector2 l2a, Vector2 l2b, out Vector2 hitPoint )
		{
			hitPoint = l1a;

			float A1 = l1b.y - l1a.y;
			float B1 = l1a.x - l1b.x;
			float C1 = A1 * l1a.x + B1 * l1a.y;

			float A2 = l2b.y - l2a.y;
			float B2 = l2a.x - l2b.x;
			float C2 = A2 * l2a.x + B2 * l2a.y;

			float delta = A1 * B2 - A2 * B1;
			if( Mathf.Approximately( delta, 0 ) ) return false;

			hitPoint.x = ( B2 * C1 - B1 * C2 ) / delta;
			hitPoint.y = ( A1 * C2 - A2 * C1 ) / delta;
			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Ray ray, Vector3 point, out Vector3 projectedPoint )
		{
			var t = point - ray.origin;
			var dl = Vector3.Cross( ray.direction, t ).magnitude;
			var dot = Vector3.Dot( ray.direction, t );
			projectedPoint = ray.origin + ray.direction * dot;
			return dl;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DistanceToLine( Ray ray, Vector3 point )
		{
			return Vector3.Cross( ray.direction, point - ray.origin ).magnitude;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Aprox( float a, float b ) => Mathf.Abs( a - b ) < 2 * float.Epsilon;


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 PlaneClosestPoint( Vector3 position, Vector3 planeNormal, Vector3 planeOrigin )
		{
			var originToPoint = position - planeOrigin;
			var originToPointDir = originToPoint.normalized;
			if( NormalsAreParallel( planeNormal, originToPointDir ) ) return planeOrigin;
			var otherComponentDir = Vector3.Cross( planeNormal, originToPointDir ).normalized;
			var planeProjectionDir = Vector3.Cross( otherComponentDir, planeNormal ).normalized;
			var projD = Vector3.Dot( originToPoint, planeProjectionDir );
			var originOffset = projD * planeProjectionDir;
			var closestPoint = planeOrigin + originOffset;
			return closestPoint;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool NormalsAreParallel( Vector3 planeNormal, Vector3 originToPoint )
		{
			return ( Aprox( planeNormal.x, originToPoint.x ) && Aprox( planeNormal.y, originToPoint.y ) && Aprox( planeNormal.z, originToPoint.z ) ) ||
					( Aprox( planeNormal.x, -originToPoint.x ) && Aprox( planeNormal.y, -originToPoint.y ) && Aprox( planeNormal.z, -originToPoint.z ) );
		}
	}
}