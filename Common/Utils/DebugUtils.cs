using UnityEngine;
using System.Collections;

public static class DebugUtils
{
	const float K_DEFAULT_ANGLE_PRECISION = Mathf.PI / 20;

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
		DebugUtils.Circle( pos, forward, up, radius, Color.blue, false );
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
    public static void Circle( Vector3 pos, Vector3 forward, Vector3 up, float radius, Color color, bool forwardGuide, float anglePrecision )
    {
        var a = forward.normalized * radius;
        if( forwardGuide ) Debug.DrawLine( pos, pos + a, color );

        for( float ang = 0; ang <= 2 * Mathf.PI; ang += anglePrecision )
        {
            var b = Quaternion.AngleAxis( anglePrecision * Mathf.Rad2Deg, up ) * a;
            Debug.DrawLine( pos + a, pos + b, color );
            a = b;
        }
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
}
