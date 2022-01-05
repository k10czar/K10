
#if USE_NEW_MATHEMATICS
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace K10
{
	public static class NewMathematicsPhysics2DUtils
	{
		const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;

		[MethodImpl( AggrInline )] public static float2 IgnoreY( this float3 v3 ) { return new float2( v3.x, v3.z ); }
		[MethodImpl( AggrInline )] public static float2 IgnoreZ( this float3 v3 ) { return new float2( v3.x, v3.y ); }
		[MethodImpl( AggrInline )] public static float3 WithZ( this float2 v3, float zValue ) { return new float3( v3.x, v3.y, zValue ); }
		[MethodImpl( AggrInline )] public static float3 WithZ0( this float2 v3 ) { return new float3( v3.x, v3.y, 0 ); }
		
		[MethodImpl( AggrInline )] public static float GetProjectedRadiusMaxSize( Camera cam, float3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Max( up, Mathf.Max( down, Mathf.Max( right, left ) ) );
		}

		[MethodImpl( AggrInline )] public static float GetProjectedRadiusAverageSize( Camera cam, float3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return ( up + down + right + left ) * .25f;
		}

		[MethodImpl( AggrInline )] public static float GetProjectedRadiusMinSize( Camera cam, float3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Min( up, Mathf.Min( down, Mathf.Min( right, left ) ) );
		}

		[MethodImpl( AggrInline )] public static void GetProjectedRadiusSizes( Camera cam, float3 worldPos, float radius, out float right, out float up, out float left, out float down )
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

		[MethodImpl( AggrInline )] public static bool Collinear( float2 p, float2 q, float2 r ) { return Aprox( Cross( q - p, r - p ), 0 ); }
		[MethodImpl( AggrInline )] public static float Cross( float2 a, float2 b ) { return a.x * b.y - a.y * b.x; }
		[MethodImpl( AggrInline )] public static bool CCW( float2 p, float2 q, float2 r ) { return Cross( q - p, r - p ) > 0; }
		[MethodImpl( AggrInline )] public static bool Aprox( float a, float b ) => math.abs( a - b ) < 2 * float.Epsilon;

		[MethodImpl( AggrInline )] public static float Angle( float2 a, float2 o, float2 b )
		{
			var oa = a - o;
			var ob = b - o;
			return math.acos( math.dot( oa, ob ) / math.sqrt( math.lengthsq( oa ) * math.lengthsq( ob ) ) );
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( float2 p, Ray2D ray, out float2 hitPoint )
		{
			var b = ray.origin + ray.direction.normalized;
			var a = ray.origin;
			return DistanceToLine( p, a, b, out hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( float2 p, float2 a, float2 b, out float2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( float2 p, float2 a, float2 b, out float2 hitPoint, float projA, float projB )
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var scale = projA / ( dx * dx + dy * dy );
			hitPoint = a + scale * ( b - a );
			return math.length( p - hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToPoint( float2 p, Ray2D ray, out float2 hitPoint, float range = float.MaxValue )
		{
			var b = ray.origin + ray.direction.normalized * range;
			var a = ray.origin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToPoint( float2 p, float2 rayOrigin, float2 rayDir, out float2 hitPoint, float range = float.MaxValue )
		{
			var b = rayOrigin + rayDir * range;
			var a = rayOrigin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

		[MethodImpl( AggrInline )] public static float DistanceToPoint( float2 p, float2 a, float2 b, out float2 hitPoint )
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

		[MethodImpl( AggrInline )] public static bool AreParellel( float2 l1a, float2 l1b, float2 l2a, float2 l2b )
		{
			var l1Dir = math.normalize( l1b - l1b );
			var l2Dir = math.normalize( l2b - l2b );
			return Aprox( l1Dir.x, l2Dir.x ) && Aprox( l1Dir.y, l2Dir.y );
		}

		[MethodImpl( AggrInline )] public static float FindScale( float2 l1a, float2 l1b )
		{
			if( Aprox( l1a.x, 0 ) ) return l1b.y / l1a.y;
			return l1b.x / l1a.x;
		}

		[MethodImpl( AggrInline )] public static bool Intersects( float2 l1a, float2 l1b, float2 l2a, float2 l2b, out float2 hitPoint )
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

		[MethodImpl( AggrInline )] public static float DistanceToLine( Ray ray, float3 point, out float3 projectedPoint )
		{
			var t = point - ( (float3)ray.origin );
			var dl = math.length( math.cross( ray.direction, t ) );
			var dot = math.dot( ray.direction, t );
			projectedPoint = ray.origin + ray.direction * dot;
			return dl;
		}

		[MethodImpl( AggrInline )] public static float DistanceToLine( Ray ray, float3 point )
		{
			return math.length( math.cross( ray.direction, point - ( (float3)ray.origin ) ) );
		}

		[MethodImpl( AggrInline )] public static float3 PlaneClosestPoint( float3 position, float3 planeNormal, float3 planeOrigin )
		{
			var originToPoint = position - planeOrigin;
			var originToPointDir = math.normalize( originToPoint );
			if( NormalsAreParallel( planeNormal, originToPointDir ) ) return planeOrigin;
			var otherComponentDir = math.normalize( math.cross( planeNormal, originToPointDir )  );
			var planeProjectionDir = math.normalize( math.cross( otherComponentDir, planeNormal ) );
			var projD = math.dot( originToPoint, planeProjectionDir );
			var originOffset = projD * planeProjectionDir;
			// var originOffset = (float3)Vector3.Project(originToPoint, planeProjectionDir);
			var closestPoint = planeOrigin + originOffset;
			return closestPoint;
		}

		[MethodImpl( AggrInline )] public static bool NormalsAreParallel( float3 planeNormal, float3 originToPoint )
		{
			return (Aprox(planeNormal.x, originToPoint.x) && Aprox(planeNormal.y, originToPoint.y) && Aprox(planeNormal.z, originToPoint.z)) ||
					(Aprox(planeNormal.x, -originToPoint.x) && Aprox(planeNormal.y, -originToPoint.y) && Aprox(planeNormal.z, -originToPoint.z));
		}
	}
}
#endif