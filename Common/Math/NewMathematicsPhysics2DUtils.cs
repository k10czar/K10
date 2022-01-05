using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v2 = Unity.Mathematics.float2;
using v3 = Unity.Mathematics.float3;
#else
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;
#endif

namespace K10
{
	public static class Physics2DUtils
	{
		const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;
		
		[MethodImpl( AggrInline )] public static float GetProjectedRadiusMaxSize( Camera cam, v3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return MathAdapter.max( up, MathAdapter.max( down, MathAdapter.max( right, left ) ) );
		}

		[MethodImpl( AggrInline )] public static float GetProjectedRadiusAverageSize( Camera cam, v3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return ( up + down + right + left ) * .25f;
		}

		[MethodImpl( AggrInline )] public static float GetProjectedRadiusMinSize( Camera cam, v3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return MathAdapter.min( up, MathAdapter.min( down, MathAdapter.min( right, left ) ) );
		}

		[MethodImpl( AggrInline )] public static void GetProjectedRadiusSizes( Camera cam, v3 worldPos, float radius, out float right, out float up, out float left, out float down )
		{
			var cwUp = MathAdapter.cross( cam.transform.forward, cam.transform.up ) * radius;
			var cwDown = MathAdapter.cross( cam.transform.forward, -cam.transform.up ) * radius;
			var cwRight = MathAdapter.cross( cam.transform.forward, cam.transform.right ) * radius;
			var cwLeft = MathAdapter.cross( cam.transform.forward, -cam.transform.right ) * radius;

			v2 center = cam.WorldToScreenPoint( worldPos ).IgnoreZ();

			v2 cUp = cam.WorldToScreenPoint( worldPos + cwUp ).IgnoreZ();
			v2 cDown = cam.WorldToScreenPoint( worldPos + cwDown ).IgnoreZ();
			v2 cRight = cam.WorldToScreenPoint( worldPos + cwRight ).IgnoreZ();
			v2 cLeft = cam.WorldToScreenPoint( worldPos + cwLeft ).IgnoreZ();

			up = MathAdapter.length( cUp - center );
			down = MathAdapter.length( cDown - center );
			right = MathAdapter.length( cRight - center );
			left = MathAdapter.length( cLeft - center );
		}

		[MethodImpl( AggrInline )] public static bool Collinear( v2 p, v2 q, v2 r ) { return Aprox( Cross( q - p, r - p ), 0 ); }
		[MethodImpl( AggrInline )] public static float Cross( v2 a, v2 b ) { return a.x * b.y - a.y * b.x; }
		[MethodImpl( AggrInline )] public static bool CCW( v2 p, v2 q, v2 r ) { return Cross( q - p, r - p ) > 0; }
		[MethodImpl( AggrInline )] public static bool Aprox( float a, float b ) => MathAdapter.abs( a - b ) < 2 * float.Epsilon;

		[MethodImpl( AggrInline )] public static float Angle( v2 a, v2 o, v2 b )
		{
			var oa = a - o;
			var ob = b - o;
			return MathAdapter.acos( MathAdapter.dot( oa, ob ) / MathAdapter.sqrt( MathAdapter.lengthsq( oa ) * MathAdapter.lengthsq( ob ) ) );
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( v2 p, Ray2D ray, out v2 hitPoint )
		{
			var b = (v2)ray.origin + MathAdapter.normalize( ray.direction );
			return DistanceToLine( p, ray.origin, b, out hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( v2 p, v2 a, v2 b, out v2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( v2 p, v2 a, v2 b, out v2 hitPoint, float projA, float projB )
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var scale = projA / ( dx * dx + dy * dy );
			hitPoint = a + scale * ( b - a );
			return MathAdapter.length( p - hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToPoint( v2 p, Ray2D ray, out v2 hitPoint, float range = float.MaxValue )
		{
			var b = (v2)ray.origin + MathAdapter.normalize( ray.direction ) * range;
			var a = ray.origin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToPoint( v2 p, v2 rayOrigin, v2 rayDir, out v2 hitPoint, float range = float.MaxValue )
		{
			var b = rayOrigin + rayDir * range;
			var a = rayOrigin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToPoint( v2 p, v2 a, v2 b, out v2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			if( Aprox( projA, 0 ) || projA < 0 )
			{
				hitPoint = a;
				return MathAdapter.length( p - a );
			}

			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			if( Aprox( projB, 0 ) || projB < 0 )
			{
				hitPoint = b;
				return MathAdapter.length( p - b );
			}

			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( AggrInline )] public static bool AreParellel( v2 l1a, v2 l1b, v2 l2a, v2 l2b )
		{
			var l1Dir = MathAdapter.normalize( l1b - l1b );
			var l2Dir = MathAdapter.normalize( l2b - l2b );
			return Aprox( l1Dir.x, l2Dir.x ) && Aprox( l1Dir.y, l2Dir.y );
		}

		[MethodImpl( AggrInline )] public static float FindScale( v2 l1a, v2 l1b )
		{
			if( Aprox( l1a.x, 0 ) ) return l1b.y / l1a.y;
			return l1b.x / l1a.x;
		}

		[MethodImpl( AggrInline )] public static bool Intersects( v2 l1a, v2 l1b, v2 l2a, v2 l2b, out v2 hitPoint )
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

		[MethodImpl( AggrInline )] public static float DistanceToLine( Ray ray, v3 point, out v3 projectedPoint )
		{
			var t = point - ( (v3)ray.origin );
			var dl = MathAdapter.length( MathAdapter.cross( ray.direction, t ) );
			var dot = MathAdapter.dot( ray.direction, t );
			projectedPoint = ray.origin + ray.direction * dot;
			return dl;
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( Ray ray, v3 point )
		{
			return MathAdapter.length( MathAdapter.cross( ray.direction, point - ( (v3)ray.origin ) ) );
		}

		[MethodImpl( AggrInline )] public static v3 PlaneClosestPoint( v3 position, v3 planeNormal, v3 planeOrigin )
		{
			var originToPoint = position - planeOrigin;
			var originToPointDir = MathAdapter.normalize( originToPoint );
			if( NormalsAreParallel( planeNormal, originToPointDir ) ) return planeOrigin;
			var otherComponentDir = MathAdapter.normalize( MathAdapter.cross( planeNormal, originToPointDir )  );
			var planeProjectionDir = MathAdapter.normalize( MathAdapter.cross( otherComponentDir, planeNormal ) );
			var projD = MathAdapter.dot( originToPoint, planeProjectionDir );
			var originOffset = projD * planeProjectionDir;
			var closestPoint = planeOrigin + originOffset;
			return closestPoint;
		}

		[MethodImpl( AggrInline )] public static bool NormalsAreParallel( v3 planeNormal, v3 originToPoint )
		{
			return (Aprox(planeNormal.x, originToPoint.x) && Aprox(planeNormal.y, originToPoint.y) && Aprox(planeNormal.z, originToPoint.z)) ||
					(Aprox(planeNormal.x, -originToPoint.x) && Aprox(planeNormal.y, -originToPoint.y) && Aprox(planeNormal.z, -originToPoint.z));
		}
	}
}