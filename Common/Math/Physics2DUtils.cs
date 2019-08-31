using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace K10
{
	public static class Physics2DUtils
	{
		public static float GetProjectedRadiusMaxSize( Camera cam, Vector3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Max( up, Mathf.Max( down, Mathf.Max( right, left ) ) );
		}

		public static float GetProjectedRadiusAverageSize( Camera cam, Vector3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return ( up + down + right + left ) * .25f;
		}

		public static float GetProjectedRadiusMinSize( Camera cam, Vector3 worldPos, float radius )
		{
			float up, down, right, left;
			GetProjectedRadiusSizes( cam, worldPos, radius, out right, out up, out left, out down );
			return Mathf.Min( up, Mathf.Min( down, Mathf.Min( right, left ) ) );
		}

		public static void GetProjectedRadiusSizes( Camera cam, Vector3 worldPos, float radius, out float right, out float up, out float left, out float down )
		{
			// var camForward = cam.transform.forward;
            // var camUp = cam.transform.up;
            // var camRight = cam.transform.right;

			// var cwUp = Vector3.Cross( camForward, camUp ) * radius;
			// var cwDown = Vector3.Cross( camForward, -camUp ) * radius;
			// var cwRight = Vector3.Cross( camForward, camRight ) * radius;
			// var cwLeft = Vector3.Cross( camForward, -camRight ) * radius;

            var cwUp = Vector3.Cross( cam.transform.forward, cam.transform.up ) * radius;
            var cwDown = Vector3.Cross( cam.transform.forward, -cam.transform.up ) * radius;
            var cwRight = Vector3.Cross( cam.transform.forward, cam.transform.right ) * radius;
            var cwLeft = Vector3.Cross( cam.transform.forward, -cam.transform.right ) * radius;

			// Debug.Log( "Mags: " + Vector3.Cross( cam.transform.forward, cam.transform.up ).magnitude + " "
            //                     + Vector3.Cross( cam.transform.forward, -cam.transform.up ).magnitude + " "
            //                     + Vector3.Cross( cam.transform.forward, cam.transform.right ).magnitude + " "
            //                     + Vector3.Cross( cam.transform.forward, -cam.transform.right ).magnitude + " " );

			// var wtsp = cam.worldToCameraMatrix;
            // Matrix4x4 mat = cam.projectionMatrix * cam.worldToCameraMatrix;

			Vector2 center = cam.WorldToScreenPoint( worldPos );

			Vector2 cUp = cam.WorldToScreenPoint( worldPos + cwUp );
			Vector2 cDown = cam.WorldToScreenPoint( worldPos + cwDown );
			Vector2 cRight = cam.WorldToScreenPoint( worldPos + cwRight );
			Vector2 cLeft = cam.WorldToScreenPoint( worldPos + cwLeft );

			// var scl = new Vector2( 1f / Screen.width, 1f / Screen.height );
            // Debug.Log( "Center: " + center + " => " + ( wtsp * worldPos ) + " => " + Vector2.Scale( center, scl ) + " => " + Test( cam, mat, worldPos )
            //         	+ "\ncUp: " + cUp + " => " + ( wtsp * ( worldPos + cwUp ) ) + " => " + Vector2.Scale( cUp, scl ) + " => " + Test( cam, mat, ( worldPos + cwUp ) )
            //         	+ "\ncDown: " + cDown + " => " + ( wtsp * ( worldPos + cwDown ) ) + " => " + Vector2.Scale( cDown, scl ) + " => " + Test( cam, mat, ( worldPos + cwDown ) )
            //         	+ "\ncRight: " + cRight + " => " + ( wtsp * ( worldPos + cwRight ) ) + " => " + Vector2.Scale( cRight, scl ) + " => " + Test( cam, mat, ( worldPos + cwRight ) )
            //         	+ "\ncLeft: " + cLeft + " => " + ( wtsp * ( worldPos + cwLeft ) ) + " => " + Vector2.Scale( cLeft, scl ) + " => " + Test( cam, mat, ( worldPos + cwLeft ) ) );

			up = ( cUp - center ).magnitude;
			down = ( cDown - center ).magnitude;
			right = ( cRight - center ).magnitude;
			left = ( cLeft - center ).magnitude;
		}

		static Vector2 Test( Camera cam, Matrix4x4 mat, Vector3 pos )
		{
            var temp = ( mat * new Vector4( pos.x, pos.y, pos.z, 1 ) );
            temp.x = ( temp.x / temp.w + 1f ) * .5f * cam.pixelWidth;
            temp.y = ( temp.y / temp.w + 1f ) * .5f * cam.pixelHeight;
			return temp;
		}

		public static bool Collinear( Vector2 p, Vector2 q, Vector2 r ) { return Mathf.Approximately( Cross( q - p, r - p ), 0 ); }
		public static float Cross( Vector2 a, Vector2 b ) { return a.x * b.y - a.y * b.x; }
		public static bool CCW( Vector2 p, Vector2 q, Vector2 r ) { return Cross( q - p, r - p ) > 0; }

		public static float Angle( Vector2 a, Vector2 o, Vector2 b )
		{
			var oa = a - o;
			var ob = b - o;
			return Mathf.Acos( Vector2.Dot( oa, ob ) / Mathf.Sqrt( oa.sqrMagnitude * ob.sqrMagnitude ) );
		}

		public static float DistanceToLine( Vector2 p, Ray2D ray, out Vector2 hitPoint )
		{
			var b = ray.origin + ray.direction.normalized;
			var a = ray.origin;
			return DistanceToLine( p, a, b, out hitPoint );
		}

		public static float DistanceToLine( Vector2 p, Vector2 a, Vector2 b, out Vector2 hitPoint )
		{
			var projA = ( b.x - a.x ) * ( p.x - a.x ) + ( b.y - a.y ) * ( p.y - a.y );
			var projB = ( a.x - b.x ) * ( p.x - b.x ) + ( a.y - b.y ) * ( p.y - b.y );
			return DistanceToLine( p, a, b, out hitPoint, projA, projB );
		}

		public static float DistanceToLine( Vector2 p, Vector2 a, Vector2 b, out Vector2 hitPoint, float projA, float projB )
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var scale = projA / ( dx * dx + dy * dy );
			hitPoint = a + scale * ( b - a );
			return ( p - hitPoint ).magnitude;
		}

		public static float DistanceToPoint( Vector2 p, Ray2D ray, out Vector2 hitPoint, float range = float.MaxValue )
		{
			var b = ray.origin + ray.direction.normalized * range;
			var a = ray.origin;
			return DistanceToPoint( p, a, b, out hitPoint );
		}

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

		public static bool AreParellel( Vector2 l1a, Vector2 l1b, Vector2 l2a, Vector2 l2b )
		{
			var l1Dir = ( l1b - l1b ).normalized;
			var l2Dir = ( l2b - l2b ).normalized;
			return Mathf.Approximately( l1Dir.x, l2Dir.x ) && Mathf.Approximately( l1Dir.y, l2Dir.y );
		}

		public static float FindScale( Vector2 l1a, Vector2 l1b )
		{
			if( Mathf.Approximately( l1a.x, 0 ) ) return l1b.y / l1a.y;
			return l1b.x / l1a.x;
		}

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

		public static float DistanceToLine( Ray ray, Vector3 point, out Vector3 projectedPoint )
		{
			var t = point - ray.origin;
			var dl = Vector3.Cross( ray.direction, point - ray.origin ).magnitude;
			var dot = Vector3.Dot( ray.direction, t );
			//			var hyp = t.magnitude;
			//			var cat = Mathf.Sqrt( hyp * hyp - dl * dl );
			//			projectedPoint = ray.origin + ray.direction * cat;
			projectedPoint = ray.origin + ray.direction * dot;
			return dl;
		}

		public static float DistanceToLine( Ray ray, Vector3 point )
		{
			return Vector3.Cross( ray.direction, point - ray.origin ).magnitude;
		}
	}
}