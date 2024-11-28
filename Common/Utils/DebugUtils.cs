using UnityEngine;
using System.Collections;
using UnityEngine.Animations;

public static class DebugUtils
{
	const float K_DEFAULT_ANGLE_PRECISION = Mathf.PI / 20;
	static readonly Color DEFAULT_COLOR = Color.green;

	public static void Rect( RectTransform rect, Color color ) 
	{ 
		var r = rect.rect;

		var a = rect.TransformPoint( r.min );
		var b = rect.TransformPoint( r.min + Vector2.right * r.width );
		var c = rect.TransformPoint( r.max );
		var d = rect.TransformPoint( r.max + Vector2.left * r.width );

		Polygon( a, b, c, d );
	}

	public static void Polygon( params Vector3[] positions ) { Polygon( Color.green, positions ); } 
	public static void Polygon( Color color, params Vector3[] positions ) 
	{
		var l = positions.Length;
		for( int i = 1; i < l; i++ ) Debug.DrawLine( positions[i-1], positions[i], color );
		Debug.DrawLine( positions[0], positions[l-1], color );
	}

	public static void FakeSphere( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color )
	{
		var cam = Camera.main;

		DebugUtils.Circle( pos, cam.transform.right, cam.transform.forward, radius, color, false );
		DebugUtils.Circle( pos, forward, up, radius, color, false );
	}

	public static void Elipse( Vector3 pos, Vector3 forward, Vector3 up, float a, float b, Color color, Vector3 scale, bool forwardGuide = false, float anglePrecision = K_DEFAULT_ANGLE_PRECISION )
	{
		var f = forward.normalized;
		if( forwardGuide ) Debug.DrawLine( pos, pos + f * a, color );

		var l = Vector3.Scale( f * a, scale );
		for( float ang = 0; ang <= 2.0001f * Mathf.PI; ang += anglePrecision )
		{
			var c1 = a * Mathf.Sin( ang );
			var c2 = b * Mathf.Cos( ang );
			var r = a * b / Mathf.Sqrt( c1*c1 + c2*c2 );
			var c = Vector3.Scale( Quaternion.AngleAxis( ang * Mathf.Rad2Deg, up ) * ( f * r ), scale );
			Debug.DrawLine( pos + l, pos + c, color );
			l = c;
		}
	}

	public static void CircleTime( Vector3 pos, float radius, Color color, float time ) { CircleTime( pos, Vector3.forward, Vector3.up, radius, color, false, time ); }
	public static void CircleTime( Vector3 pos, float radius, Color color, float anglePrecision, float time ) { CircleTime( pos, Vector3.forward, Vector3.up, radius, color, false, anglePrecision, time ); }
	public static void CircleTime( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, bool forwardGuide, float time ) { CircleTime( pos, forward, up, radius, color, forwardGuide, K_DEFAULT_ANGLE_PRECISION, time ); }
    public static void CircleTime( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, bool forwardGuide, float anglePrecision, float time )
    {
        var a = forward.normalized * radius;
        if( forwardGuide ) Debug.DrawLine( pos, pos + a, color, time );

        for( float ang = 0; ang <= 2 * Mathf.PI; ang += anglePrecision )
        {
            var b = Quaternion.AngleAxis( anglePrecision * Mathf.Rad2Deg, up ) * a;
            Debug.DrawLine( pos + a, pos + b, color, time );
            a = b;
        }
	}

	public static void Circle( Vector3 pos, float radius, Color color ) { Circle( pos, Vector3.forward, Vector3.up, radius, color, false ); }
	public static void Circle( Vector3 pos, float radius, Color color, float anglePrecision ) { Circle( pos, Vector3.forward, Vector3.up, radius, color, false, anglePrecision ); }
	public static void Circle( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, bool forwardGuide ) { Circle( pos, forward, up, radius, color, forwardGuide, K_DEFAULT_ANGLE_PRECISION ); }
	public static void Circle(Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, bool forwardGuide, float anglePrecision)
	{
		var a = forward.normalized * radius;
		if (forwardGuide) Debug.DrawLine(pos, pos + a, color);

		for (float ang = 0; ang <= 2 * Mathf.PI; ang += anglePrecision)
		{
			var b = Quaternion.AngleAxis(anglePrecision * Mathf.Rad2Deg, up) * a;
			Debug.DrawLine(pos + a, pos + b, color);
			a = b;
		}
	}

	private static Vector2 GetElipsePoint(float t, float a, float b) => new Vector2( a * Mathf.Cos( t ),  b * Mathf.Sin( t ) );
	private static Vector3 To3d(Vector2 v2d, Vector3 origin, Vector3 v3dAxisX, Vector3 v3dAxisY) => origin + v2d.x * v3dAxisX + v2d.y * v3dAxisY;

	public static void Elipse( Vector3 origin, float r, float R, Vector3 majorAxis, Vector3 normal, Color color, float anglePrecision = K_DEFAULT_ANGLE_PRECISION )
	{
		var f = Mathf.Sqrt(R * R - r * r);
		var faxis = f * majorAxis;
		var f1 = origin + faxis;
		var f2 = origin - faxis;

		var minorAxis = Vector3.Cross( majorAxis, normal );
		var p2d = GetElipsePoint( 0, R, r );
		var a = To3d( p2d, origin, majorAxis, minorAxis );

		for (float ang = anglePrecision; ang <= 2 * Mathf.PI; ang += anglePrecision)
		{
			p2d = GetElipsePoint(ang, R, r);
			var b = To3d(p2d, origin, majorAxis, minorAxis);
			Debug.DrawLine(a, b, color);
			a = b;
		}

		p2d = GetElipsePoint(2 * Mathf.PI, R, r);
		Debug.DrawLine(a, To3d(p2d, origin, majorAxis, minorAxis), color);
	}

	public static void Cone( Vector3 pos, Vector3 forward, Vector3 up, float angle, float distance, Color color )
	{
		var first = Quaternion.AngleAxis( -angle / 2 * Mathf.Rad2Deg, up ) * ( forward.normalized * distance );
		var last = Quaternion.AngleAxis( angle / 2 * Mathf.Rad2Deg, up ) * ( forward.normalized * distance );
		Debug.DrawLine( pos, pos + first, color );
		Debug.DrawLine( pos, pos + last, color );
		var a = first;

		for( float ang = K_DEFAULT_ANGLE_PRECISION; ang < angle; ang += K_DEFAULT_ANGLE_PRECISION )
		{
			var b = Quaternion.AngleAxis( ang * Mathf.Rad2Deg, up ) * first;
			Debug.DrawLine( pos + a, pos + b, color );
			a = b;
		}

		if( a != last ) Debug.DrawLine( pos + a, pos + last, color );
	}

    public static void Triangle( Vector3 pos, Vector3 forward, Vector3 up, float angle, float distance, Color color )
    {
        var first = Quaternion.AngleAxis( -angle / 2 * Mathf.Rad2Deg, up ) * (forward.normalized * distance);
        var last = Quaternion.AngleAxis( angle / 2 * Mathf.Rad2Deg, up ) * (forward.normalized * distance);
        Debug.DrawLine( pos, pos + first, color );
        Debug.DrawLine( pos, pos + last, color );

        Debug.DrawLine( pos + first, pos + last, color );
    }

    public static void Diamond( Vector3 pos, float angle, float distance, Color color )
    {
        var first1 = Quaternion.AngleAxis( -angle / 2 * Mathf.Rad2Deg, Vector3.forward ) * (-Vector3.up.normalized * distance);
        var last1 = Quaternion.AngleAxis( angle / 2 * Mathf.Rad2Deg, Vector3.forward ) * (-Vector3.up.normalized * distance);
        Debug.DrawLine( pos, pos + first1, color );
        Debug.DrawLine( pos, pos + last1, color );

        var first2 = Quaternion.AngleAxis( -angle / 2 * Mathf.Rad2Deg, Vector3.right ) * (-Vector3.up.normalized * distance);
        var last2 = Quaternion.AngleAxis( angle / 2 * Mathf.Rad2Deg, Vector3.right ) * (-Vector3.up.normalized * distance);
        Debug.DrawLine( pos, pos + first2, color );
        Debug.DrawLine( pos, pos + last2, color );

        Debug.DrawLine( pos + first1, pos + first2, color );
        Debug.DrawLine( pos + last1, pos + last2, color );

        Debug.DrawLine( pos + last1, pos + first2, color );
        Debug.DrawLine( pos + last2, pos + first1, color );
    }

    public static void X( Vector3 pos, float radius, Color color, float time = 0 ) { X( pos, Vector3.forward, Vector3.up, radius, color, time ); }
	public static void X(Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, float time = 0)
	{
		var a = forward.normalized * radius;

		Debug.DrawLine(pos + Quaternion.AngleAxis(45, up) * a, pos + Quaternion.AngleAxis(225, up) * a, color, time );
		Debug.DrawLine(pos + Quaternion.AngleAxis(315, up) * a, pos + Quaternion.AngleAxis(135, up) * a, color, time );
	}

	public static void TravelArrow( Vector3 origin, Vector3 destination, float tipScale, Color color, float time = 0 ) { TravelArrow( origin, destination, Vector3.up, tipScale, color ); }
	public static void TravelArrow( Vector3 origin, Vector3 destination, Vector3 up, float tipScale, Color color ) { TravelArrow( origin, destination, up, tipScale, color, 30 ); }
	public static void TravelArrow( Vector3 origin, Vector3 destination, Vector3 up, float tipScale, Color color, float angle, float time = 0 ) { TravelArrow( origin, destination, up, tipScale, color, color, angle ); }
	public static void TravelArrow( Vector3 origin, Vector3 destination, Vector3 up, float tipScale, Color color, Color tipColor, float angle, float time = 0 )
	{
		var travel = destination - origin;
		var dir = travel.normalized;
		var a = dir * tipScale;

		Debug.DrawLine( origin, destination, color, time );
		Debug.DrawLine( destination, destination + Quaternion.AngleAxis( 180 - angle, up ) * a, tipColor, time );
		Debug.DrawLine( destination, destination + Quaternion.AngleAxis( 180 + angle, up ) * a, tipColor, time );
	}

	public static void DistanceMeasure( Vector3 a, Vector3 b, Vector3 up, Color color, float radius = 1 )
	{
		Debug.DrawLine( a, b, color );
		var dir = ( b - a );
		var endDir = Vector3.Cross( dir, up ).normalized;
		radius *= .5f;
		Debug.DrawLine( a + endDir * radius, a - endDir * radius, color );
		Debug.DrawLine( b + endDir * radius, b - endDir * radius, color );
	}

	public static void Arrow( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color )
	{
		var a = forward.normalized * radius;

		Debug.DrawLine( pos + Quaternion.AngleAxis( 90, up ) * a, pos + Quaternion.AngleAxis( 0, up ) * a, color );
		Debug.DrawLine( pos + Quaternion.AngleAxis( 0, up ) * a, pos + Quaternion.AngleAxis( 270, up ) * a, color );
		Debug.DrawLine( pos + Quaternion.AngleAxis( 0, up ) * a, pos + Quaternion.AngleAxis( 180, up ) * a, color );
	}

	public static void Arrow( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, float duration )
	{
		var a = forward.normalized * radius;

		Debug.DrawLine( pos + Quaternion.AngleAxis( 90, up ) * a, pos + Quaternion.AngleAxis( 0, up ) * a, color, duration );
		Debug.DrawLine( pos + Quaternion.AngleAxis( 0, up ) * a, pos + Quaternion.AngleAxis( 270, up ) * a, color, duration );
		Debug.DrawLine( pos + Quaternion.AngleAxis( 0, up ) * a, pos + Quaternion.AngleAxis( 180, up ) * a, color, duration );
	}

	public static void PointTriAxis( Vector3 point, Color color, float ptSize = .1f )
	{
		Debug.DrawRay(point, Vector3.forward * ptSize, color);
		Debug.DrawRay(point, Vector3.up * ptSize, color);
		Debug.DrawRay(point, Vector3.right * ptSize, color);
	}

    public static void WireBox( Vector3 center ) => WireBox( center, Quaternion.identity, Vector3.one, DEFAULT_COLOR );
    public static void WireBox( Vector3 center, Quaternion rotation ) => WireBox( center, rotation, Vector3.one, DEFAULT_COLOR );
    public static void WireBox( Vector3 center, Vector3 dimensions ) => WireBox( center, Quaternion.identity, dimensions, DEFAULT_COLOR );
    public static void WireBox( Vector3 center, Vector3 dimensions, Color color ) => WireBox( center, Quaternion.identity, dimensions, color );
    public static void WireBox( Vector3 center, Quaternion rotation, Vector3 dimensions ) => WireBox( center, rotation, dimensions, DEFAULT_COLOR );
    public static void WireBox( Vector3 center, Quaternion rotation, Vector3 dimensions, Color color )
    {
		var f = rotation * Vector3.forward * dimensions.z;
		var u = rotation * Vector3.up * dimensions.y;
		var r = rotation * Vector3.right * dimensions.x;
		var ob = center - ( f + u + r ) * .5f; //Origin Bottom
		var ot = center - ( f + r - u ) * .5f; //Origin Top
		var obf = ob + f;
		var obfr = obf + r;
		var obr = ob + r;
		var otf = ot + f;
		var otfr = otf + r;
		var otr = ot + r;

		//Bottom Quad
		Debug.DrawLine( ob, obf, color);
		Debug.DrawLine( obf, obfr, color);
		Debug.DrawLine( obfr, obr, color);
		Debug.DrawLine( obr, ob, color);

		//Top Quad
		Debug.DrawLine( ot, otf, color);
		Debug.DrawLine( otf, otfr, color);
		Debug.DrawLine( otfr, otr, color);
		Debug.DrawLine( otr, ot, color);

		//Bottom Top edges
		Debug.DrawLine( ob, ot, color);
		Debug.DrawLine( obf, otf, color);
		Debug.DrawLine( obfr, otfr, color);
		Debug.DrawLine( obr, otr, color);
    }

    public static class Gizmos
	{
		public static void Bar( float fill, Vector3 origin )
		{
			Bar( fill, origin, new Vector3( .1f, 1, .1f ), Axis.Y );
		}
		
		public static void Bar( float fill, Vector3 origin, Color color )
		{
			Bar( fill, origin, new Vector3( .1f, 1, .1f ), color, Axis.Y );
		}
		
		public static void Bar( float fill, Vector3 origin, Vector3 size, Axis fillAxis = Axis.Y )
		{
			var scaledBar = size;
			var fillCenter = origin;
			var fullCenter = origin;
			if( ( fillAxis & Axis.X ) != 0 ) 
			{
				fullCenter.x += size.x / 2;
				scaledBar.x *= fill;
				fillCenter.x += scaledBar.x / 2;
			}
			if( ( fillAxis & Axis.Y ) != 0 ) 
			{
				fullCenter.y += size.y / 2;
				scaledBar.y *= fill;
				fillCenter.y += scaledBar.y / 2;
			}
			if( ( fillAxis & Axis.Z ) != 0 ) 
			{
				fullCenter.z += size.z / 2;
				scaledBar.z *= fill;
				fillCenter.z += scaledBar.z / 2;
			}
			UnityEngine.Gizmos.DrawWireCube( fullCenter, size );
			UnityEngine.Gizmos.DrawCube( fillCenter, scaledBar );
		}

		public static void Bar( float fill, Vector3 origin, Vector3 size, Color color, Axis fillAxis = Axis.Y )
		{
			GizmosColorManager.New( color );
			Bar( fill, origin, size, fillAxis );
			GizmosColorManager.Revert();
		}


		public static void WireBox( Vector3 center ) => WireBox( center, Quaternion.identity, Vector3.one );
		public static void WireBox( Vector3 center, Vector3 dimensions ) => WireBox( center, Quaternion.identity, dimensions );
		public static void WireBox( Vector3 center, Quaternion rotation ) => WireBox( center, rotation, Vector3.one );
		public static void WireBox( Vector3 center, Quaternion rotation, Vector3 dimensions )
		{
			var f = rotation * Vector3.forward * dimensions.z;
			var u = rotation * Vector3.up * dimensions.y;
			var r = rotation * Vector3.right * dimensions.x;
			var ob = center - ( f + u + r ) * .5f; //Origin Bottom
			var ot = center - ( f + r - u ) * .5f; //Origin Top
			var obf = ob + f;
			var obfr = obf + r;
			var obr = ob + r;
			var otf = ot + f;
			var otfr = otf + r;
			var otr = ot + r;

			//Bottom Quad
			UnityEngine.Gizmos.DrawLine( ob, obf );
			UnityEngine.Gizmos.DrawLine( obf, obfr );
			UnityEngine.Gizmos.DrawLine( obfr, obr );
			UnityEngine.Gizmos.DrawLine( obr, ob );

			//Top Quad
			UnityEngine.Gizmos.DrawLine( ot, otf );
			UnityEngine.Gizmos.DrawLine( otf, otfr );
			UnityEngine.Gizmos.DrawLine( otfr, otr );
			UnityEngine.Gizmos.DrawLine( otr, ot );

			//Bottom Top edges
			UnityEngine.Gizmos.DrawLine( ob, ot );
			UnityEngine.Gizmos.DrawLine( obf, otf );
			UnityEngine.Gizmos.DrawLine( obfr, otfr );
			UnityEngine.Gizmos.DrawLine( obr, otr );
		}

		public static void WireBox( Vector3 center, Quaternion rotation, Vector3 dimensions, Color color )
		{
			GizmosColorManager.New( color );
			WireBox( center, rotation, dimensions );
			GizmosColorManager.Revert();
		}
	}
}
