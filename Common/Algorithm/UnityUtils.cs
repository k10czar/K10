using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Algorithm
{
    #region Transform
    public static void BakeScale( Transform t )
    {
        Vector3 oldScale = t.localScale;

        foreach( Transform child in t )
        {
            child.localScale = new Vector3( child.localScale.x * oldScale.x, child.localScale.y * oldScale.y, child.localScale.z * oldScale.z );
            child.localPosition = new Vector3( child.localPosition.x * oldScale.x, child.localPosition.y * oldScale.y, child.localPosition.z * oldScale.z );
        }

        foreach( var col in t.GetComponents<BoxCollider>() )
        {
            col.size = new Vector3( col.size.x * oldScale.x, col.size.y * oldScale.y, col.size.z * oldScale.z );
            col.center = new Vector3( col.center.x * oldScale.x, col.center.y * oldScale.y, col.center.z * oldScale.z );
        }

        foreach( var col in t.GetComponents<BoxCollider2D>() )
        {
            col.size = new Vector2( col.size.x * oldScale.x, col.size.y * oldScale.y );
			col.offset = new Vector2( col.offset.x * oldScale.x, col.offset.y * oldScale.y );
        }

        t.localScale = Vector3.one;
    }
	#endregion Transform

	#region Rect
	public static void InflateRelative( ref Rect r, float x, float y ) { Inflate( ref r, r.width * x, r.height * y ); }
    public static void InflateRelative( ref Rect r, Vector2 inflation ) { InflateRelative( ref r, inflation.x, inflation.y ); }
    public static void InflateRelative( ref Rect r, float inflation ) { InflateRelative( ref r, inflation, inflation ); }

    public static void Inflate( ref Rect r, float x, float y ) { r.Set( r.x - ( x / 2 ), r.y - ( y / 2 ), r.width + x, r.height + y ); }
    public static void Inflate( ref Rect r, Vector2 inflation ) { Inflate( ref r, inflation.x, inflation.y ); }
    public static void Inflate( ref Rect r, float inflation ) { Inflate( ref r, inflation, inflation ); }

    #endregion Rect

    #region Bounds
    public static Bounds MinMaxBounds( Vector3 min, Vector3 max ) { var size = max - min; return new Bounds( min + size / 2, size ); }
    #endregion Bounds
	
	public static float DistanceToRay( Vector3 rayOrigin, Vector3 rayDir, Vector3 point )
	{
		return DistanceToRay( new Ray( rayOrigin, rayDir ), point );
	}

	public static float DistanceToRay( Ray ray, Vector3 point )
	{
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}

	public static float DistancePointLine( Vector3 point, Vector3 lineStart, Vector3 lineEnd )
	{
		return Vector3.Magnitude( ProjectPointLine( point, lineStart, lineEnd ) - point );
	}
	
	public static Vector3 ProjectPointLine( Vector3 point, Vector3 lineStart, Vector3 lineEnd )
	{
		Vector3 rhs = point - lineStart;
		Vector3 vector2 = lineEnd - lineStart;
		float magnitude = vector2.magnitude;
		Vector3 lhs = vector2;
		if (magnitude > 1E-06f)
		{
			lhs = (Vector3)(lhs / magnitude);
		}
		float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
		return (lineStart + ((Vector3)(lhs * num2)));
	}

	public static void SetLayer( GameObject go, string layer ) { SetLayer( go, SortingLayer.NameToID( layer ) ); }
	public static void SetLayer( GameObject go, int layerID )
	{
		foreach( var rend in go.GetComponentsInChildren<Renderer>( true ) ) { rend.sortingLayerID = layerID; }
	}


	public static void SetSortingOrderUpTo( GameObject go, int order ) { int min, max; SetSortingOrderUpTo( go, order, out min, out max ); }
	public static void SetSortingOrderUpTo( GameObject go, int order, out int minOrder, out int maxOrder )
	{
		int min = int.MaxValue;
		int max = int.MinValue;

		foreach( var rend in go.GetComponentsInChildren<Renderer>( true ) ) { var o = rend.sortingOrder; min = Mathf.Min( min, o ); max = Mathf.Max( max, o ); }
		foreach( var rend in go.GetComponentsInChildren<RendererSortingSetter>( true ) ) { var o = rend._sortingOrder; min = Mathf.Min( min, o ); max = Mathf.Max( max, o ); }

		int delta = order + 1 - min;
		minOrder = min + delta;
		maxOrder = max + delta;

		foreach( var rend in go.GetComponentsInChildren<Renderer>( true ) ) { rend.sortingOrder += delta; }
		foreach( var rend in go.GetComponentsInChildren<RendererSortingSetter>( true ) ) { rend._sortingOrder += delta; }
	}

	public static bool SafeEquals( System.Object a, System.Object b ) { return ( a == null && b == null ) || ( a != null && a.Equals( b ) ); }

	public static void SetZBefore( GameObject go, float z ) { float minZ, maxZ; SetZBefore( go, z, out minZ, out maxZ ); }
	public static void SetZBefore( GameObject go, float z, out float min, out float max )
	{
		float minZ = float.MaxValue;
		float maxZ = float.MinValue;

		foreach( var t in go.GetComponentsInChildren<Transform>( true ) ) { var pz = t.position.z; minZ = Mathf.Min( minZ, pz ); maxZ = Mathf.Max( maxZ, pz ); }

		float deltaZ = z - .1f - maxZ;
		min = minZ + deltaZ;
		max = maxZ + deltaZ;

		go.transform.position += new Vector3( 0, 0, deltaZ );
	}
}
