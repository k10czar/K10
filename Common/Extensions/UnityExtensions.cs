using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using UnityEngine.SceneManagement;

public static class K10UnityExtensions
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;

	[MethodImpl( AggrInline )] public static List<T> Scrambled<T>( this IEnumerable<T> collection )
	{
		var olist = new List<T>( collection );
		var retList = new List<T>();

		while( olist.Count > 0 )
			// retList.Add( olist.RandomPop() );
		{
			var id = K10Random.Less( olist.Count );
			var element = olist[id];
			retList.Add( element );
			olist.RemoveAt( id );
		}

		return retList;
	}


	#region Debug
	public static void Debug( this Rect r )
	{
		var tl = ( Vector3.right * r.x ) + ( Vector3.up * r.yMax );
		var tr = ( Vector3.right * r.xMax ) + ( Vector3.up * r.yMax );
		var bl = ( Vector3.right * r.x ) + ( Vector3.up * r.y );
		var br = ( Vector3.right * r.xMax ) + ( Vector3.up * r.y );

		Gizmos.DrawLine( tr, tl );
		Gizmos.DrawLine( tr, br );
		Gizmos.DrawLine( br, bl );
		Gizmos.DrawLine( tl, bl );
	}
	public static void Debug( this Rect r, float rotation )
	{
		//                var rot = Quaternion.Euler( Vector3.forward * rotation );

		var center = new Vector3( r.center.x, r.center.y, 0 );
		var right = ( Vector3.right * r.width / 2 * Mathf.Cos( rotation ) ) + ( Vector3.up * r.width / 2 * Mathf.Sin( rotation ) );
		var up = ( Vector3.right * r.height / 2 * Mathf.Sin( rotation ) ) - ( Vector3.up * r.height / 2 * Mathf.Cos( rotation ) );
		var tl = center - right + up;
		var tr = center + right + up;
		var bl = center - right - up;
		var br = center + right - up;

		Gizmos.DrawLine( tr, tl );
		Gizmos.DrawLine( tr, br );
		Gizmos.DrawLine( br, bl );
		Gizmos.DrawLine( tl, bl );
	}

	public static void Debug( this Vector2 v, float size )
	{
		var s2 = size;
		var tl = ( Vector3.right * ( v.x - s2 ) ) + ( Vector3.up * ( v.y + s2 ) );
		var tr = ( Vector3.right * ( v.x + s2 ) ) + ( Vector3.up * ( v.y + s2 ) );
		var bl = ( Vector3.right * ( v.x - s2 ) ) + ( Vector3.up * ( v.y - s2 ) );
		var br = ( Vector3.right * ( v.x + s2 ) ) + ( Vector3.up * ( v.y - s2 ) );

		Gizmos.DrawLine( tl, br );
		Gizmos.DrawLine( tr, bl );
	}
	#endregion Debug

	#region Rect
	[MethodImpl( AggrInline )] public static bool Intersect( this Rect r, Rect other ) { return !( r.xMax < other.xMin || r.yMax < other.yMin || other.xMax < r.xMin || other.yMax < r.yMin ); }

	[MethodImpl( AggrInline )] public static Rect Intersection( this Rect r, Rect other )
	{
		if( r.Intersect( other ) ) return new Rect( r.x, r.y, 0f, 0f );

		float x = Mathf.Max( r.x, other.x );
		float y = Mathf.Max( r.y, other.y );
		float xMax = Mathf.Min( r.xMax, other.xMax );
		float yMax = Mathf.Min( r.xMax, other.xMax );
		return new Rect( x, y, xMax - x, yMax - y );
	}

	[MethodImpl( AggrInline )] public static Rect MaxWitdh( this Rect r, float width ) { return new Rect( r.x, r.y, Mathf.Max( r.width, width ), r.height ); }
	[MethodImpl( AggrInline )] public static Rect MinWitdh( this Rect r, float width ) { return new Rect( r.x, r.y, Mathf.Min( r.width, width ), r.height ); }
	[MethodImpl( AggrInline )] public static Rect MaxHeight( this Rect r, float height ) { return new Rect( r.x, r.y, r.width, Mathf.Max( r.height, height ) ); }
	[MethodImpl( AggrInline )] public static Rect MinHeight( this Rect r, float height ) { return new Rect( r.x, r.y, r.width, Mathf.Min( r.height, height ) ); }

	[MethodImpl( AggrInline )] public static Rect HorizontalSlice( this Rect r, int id, int slices, float spacing = 2 ) { return new Rect( r.x, r.y + r.height * id / slices, r.width, ( r.height - ( spacing * ( slices - 1 ) ) ) / slices ); }
	[MethodImpl( AggrInline )] public static Rect VerticalSlice( this Rect r, int id, int slices, float sliceSize = 1, float spacing = 2 ) { return new Rect( r.x + r.width * id / slices, r.y, sliceSize * ( ( r.width - ( spacing * ( slices - 1 ) ) ) / slices ) + spacing * ( sliceSize - 1 ), r.height ); }
	[MethodImpl( AggrInline )] public static Rect CutTop( this Rect r, float height ) { return new Rect( r.x, r.y + height, r.width, r.height - height ); }
	[MethodImpl( AggrInline )] public static Rect CutBottom( this Rect r, float height ) { return new Rect( r.x, r.y, r.width, r.height - height ); }
	[MethodImpl( AggrInline )] public static Rect CutLeft( this Rect r, float width ) { return new Rect( r.x + width, r.y, r.width - width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect CutRight( this Rect r, float width ) { return new Rect( r.x, r.y, r.width - width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect RequestHeight( this Rect r, float height ) { return new Rect( r.x, r.y + ( r.height - height ) / 2, r.width, height ); }
	[MethodImpl( AggrInline )] public static Rect RequestWidth( this Rect r, float width ) { return new Rect( r.x + ( r.width - width ) / 2, r.y, width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect Move( this Rect r, Vector2 offset ) { return new Rect( r.x + offset.x, r.y + offset.y, r.width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect Move( this Rect r, float x, float y ) { return new Rect( r.x + x, r.y + y, r.width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect MoveUp( this Rect r, float distance ) { return new Rect( r.x, r.y - distance, r.width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect MoveDown( this Rect r, float distance ) { return new Rect( r.x, r.y + distance, r.width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect MoveLeft( this Rect r, float distance ) { return new Rect( r.x - distance, r.y, r.width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect MoveRight( this Rect r, float distance ) { return new Rect( r.x + distance, r.y, r.width, r.height ); }

	[MethodImpl( AggrInline )] public static Rect RequestTop( this Rect r, float height ) { return CutBottom( r, r.height - height ); }
	[MethodImpl( AggrInline )] public static Rect RequestBottom( this Rect r, float height ) { return CutTop( r, r.height - height ); }

	[MethodImpl( AggrInline )] public static Rect RequestLeft( this Rect r, float width ) { return CutRight( r, r.width - width ); }
	[MethodImpl( AggrInline )] public static Rect RequestRight( this Rect r, float width ) { return CutLeft( r, r.width - width ); }

	[MethodImpl( AggrInline )] public static Rect GetLineTop( this ref Rect rect, float lineHeight, float spacing = 0 )
	{
		var line = rect.RequestTop( lineHeight + spacing / 2 );
		rect = rect.CutTop( lineHeight + spacing );
		return line;
	}
	[MethodImpl( AggrInline )] public static Rect GetLineBottom( this ref Rect rect, float lineHeight, float spacing = 0 )
	{
		var line = rect.RequestBottom( lineHeight + spacing / 2 );
		rect = rect.CutBottom( lineHeight + spacing );
		return line;
	}

	[MethodImpl( AggrInline )] public static Rect GetColumnLeft( this ref Rect rect, float width, float spacing = 0 )
	{
		var column = rect.RequestLeft( width );
		rect = rect.CutLeft( width + spacing );
		return column;
	}

	[MethodImpl( AggrInline )] public static Rect GetColumnRight( this ref Rect rect, float width, float spacing = 0 )
	{
		var column = rect.RequestRight( width );
		rect = rect.CutRight( width + spacing );
		return column;
	}
	
	[MethodImpl(AggrInline)] public static Vector2 Clamp( this Rect thisRect , Vector2 pos )
	{
		pos.x = MathAdapter.clamp(pos.x, thisRect.xMin, thisRect.xMax);
		pos.y = MathAdapter.clamp(pos.y, thisRect.yMin, thisRect.yMax);
		return pos;
	}
	
	[MethodImpl(AggrInline)] public static Vector2 SoftClamp( this Rect rect, Vector2 pos, float softMargin = .2f, float virtualMargin = 2f )
	{
		pos.x = MathAdapter.SoftClamp( pos.x, rect.xMin, rect.xMax, softMargin, virtualMargin );
		pos.y = MathAdapter.SoftClamp( pos.y, rect.yMin, rect.yMax, softMargin, virtualMargin );
		return pos;
	}

	[MethodImpl( AggrInline )] public static Rect ExpandTop( this Rect r, float height ) { return new Rect( r.x, r.y - height, r.width, r.height + height ); }
	[MethodImpl( AggrInline )] public static Rect ExpandBottom( this Rect r, float height ) { return new Rect( r.x, r.y, r.width, r.height + height ); }
	[MethodImpl( AggrInline )] public static Rect ExpandLeft( this Rect r, float width ) { return new Rect( r.x - width, r.y, r.width + width, r.height ); }
	[MethodImpl( AggrInline )] public static Rect ExpandRight( this Rect r, float width ) { return new Rect( r.x, r.y, r.width + width, r.height ); }

	[MethodImpl( AggrInline )] public static Rect Inflated( this Rect r, float x, float y ) { return new Rect( r.x - ( x / 2 ), r.y - ( y / 2 ), r.width + x, r.height + y ); }
	[MethodImpl( AggrInline )] public static Rect Inflated( this Rect r, Vector2 inflation ) { return r.Inflated( inflation.x, inflation.y ); }
	[MethodImpl( AggrInline )] public static Rect Inflated( this Rect r, float inflation ) { return r.Inflated( inflation, inflation ); }

	[MethodImpl( AggrInline )] public static Rect Shrink( this Rect r, float x, float y ) { return r.Inflated( -x, -y ); }
	[MethodImpl( AggrInline )] public static Rect Shrink( this Rect r, Vector2 shrink ) { return r.Shrink( shrink.x, shrink.y ); }
	[MethodImpl( AggrInline )] public static Rect Shrink( this Rect r, float shrink ) { return r.Shrink( shrink, shrink ); }

	[MethodImpl( AggrInline )] public static Rect HorizontalInflated( this Rect r, float inflation ) { return r.Inflated( inflation, 0 ); }
	[MethodImpl( AggrInline )] public static Rect VerticalInflated( this Rect r, float inflation ) { return r.Inflated( 0, inflation ); }

	[MethodImpl( AggrInline )] public static Rect HorizontalShrink( this Rect r, float shrink ) { return r.Shrink( shrink, 0 ); }
	[MethodImpl( AggrInline )] public static Rect VerticalShrink( this Rect r, float shrink ) { return r.Shrink( 0, shrink ); }

	public static string HierarchyName( this GameObject go ) { return HierarchyName( go.transform ); }
	public static string HierarchyName( this Transform t )
	{
		var name = t.name;
		t = t.parent;

		while( t != null )
		{
			name = t.name + "." + name;
			t = t.parent;
		}

		return SceneManager.GetActiveScene().name + ":" + name;
	}
	
	public static Transform Ancestor( this Transform t )
	{
		if( t == null ) return null;

		while( t.parent != null )
		{
			t = t.parent;
		}

		return t;
	}

	[MethodImpl( AggrInline )] public static IEventTrigger<T, K, J> UntilLifeTime<T, K, J>( this Component c, IEventTrigger<T, K, J> t ) { return UntilValidator<T, K, J>( c, t ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T, K> UntilLifeTime<T, K>( this Component c, IEventTrigger<T, K> t ) { return UntilValidator<T, K>( c, t ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T> UntilLifeTime<T>( this Component c, IEventTrigger<T> t ) { return UntilValidator<T>( c, t ); }
	[MethodImpl( AggrInline )] public static IEventTrigger UntilLifeTime( this Component c, IEventTrigger t ) { return UntilValidator( c, t ); }

	[MethodImpl( AggrInline )] public static IEventTrigger<T, K, J> UntilLifeTime<T, K, J>( this Component c, System.Action<T, K, J> act ) { return UntilValidator<T, K, J>( c, act ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T, K> UntilLifeTime<T, K>( this Component c, System.Action<T, K> act ) { return UntilValidator<T, K>( c, act ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T> UntilLifeTime<T>( this Component c, System.Action<T> act ) { return UntilValidator<T>( c, act ); }
	[MethodImpl( AggrInline )] public static IEventTrigger UntilLifeTime( this Component c, System.Action act ) { return UntilValidator( c, act ); }

	// [MethodImpl( AggrInline )] public static IEventTrigger<T, K, J> UntilLifeTime<T, K, J>( this Component c, IEventTrigger<T, K, J> t ) { return new ConditionalEventListener<T, K, J>( t, () => ( c != null ) && t.IsValid ); }
	// [MethodImpl( AggrInline )] public static IEventTrigger<T, K> UntilLifeTime<T, K>( this Component c, IEventTrigger<T, K> t ) { return new ConditionalEventListener<T, K>( t, () => ( c != null ) && t.IsValid ); }
	// [MethodImpl( AggrInline )] public static IEventTrigger<T> UntilLifeTime<T>( this Component c, IEventTrigger<T> t ) { return new ConditionalEventListener<T>( t, () => ( c != null ) && t.IsValid ); }
	// [MethodImpl( AggrInline )] public static IEventTrigger UntilLifeTime( this Component c, IEventTrigger t ) { return new ConditionalEventListener( t, () => ( c != null ) && t.IsValid ); }

	// [MethodImpl( AggrInline )] public static IEventTrigger<T, K, J> UntilLifeTime<T, K, J>( this Component c, System.Action<T, K, J> act ) { return new ConditionalEventListener<T, K, J>( act, c.IsAlive ); }
	// [MethodImpl( AggrInline )] public static IEventTrigger<T, K> UntilLifeTime<T, K>( this Component c, System.Action<T, K> act ) { return new ConditionalEventListener<T, K>( act, c.IsAlive ); }
	// [MethodImpl( AggrInline )] public static IEventTrigger<T> UntilLifeTime<T>( this Component c, System.Action<T> act ) { return new ConditionalEventListener<T>( act, c.IsAlive ); }
	// [MethodImpl( AggrInline )] public static IEventTrigger UntilLifeTime( this Component c, System.Action act ) { return new ConditionalEventListener( act, c.IsAlive ); }
	[MethodImpl( AggrInline )] public static bool IsAlive( this Component c ) { return ( c != null ); }


	[MethodImpl( AggrInline )] public static IEventTrigger<T, K, J> UntilValidator<T, K, J>( this Component c, IEventTrigger<T, K, J> t ) { return c.gameObject.EventRelay().LifetimeValidator.Validated<T, K, J>( t ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T, K> UntilValidator<T, K>( this Component c, IEventTrigger<T, K> t ) { return c.gameObject.EventRelay().LifetimeValidator.Validated<T, K>( t ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T> UntilValidator<T>( this Component c, IEventTrigger<T> t ) { return c.gameObject.EventRelay().LifetimeValidator.Validated<T>( t ); }
	[MethodImpl( AggrInline )] public static IEventTrigger UntilValidator( this Component c, IEventTrigger t ) { return c.gameObject.EventRelay().LifetimeValidator.Validated( t ); }

	[MethodImpl( AggrInline )] public static IEventTrigger<T, K, J> UntilValidator<T, K, J>( this Component c, System.Action<T, K, J> act ) { return c.gameObject.EventRelay().LifetimeValidator.Validated<T, K, J>( act ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T, K> UntilValidator<T, K>( this Component c, System.Action<T, K> act ) { return c.gameObject.EventRelay().LifetimeValidator.Validated<T, K>( act ); }
	[MethodImpl( AggrInline )] public static IEventTrigger<T> UntilValidator<T>( this Component c, System.Action<T> act ) { return c.gameObject.EventRelay().LifetimeValidator.Validated<T>( act ); }
	[MethodImpl( AggrInline )] public static IEventTrigger UntilValidator( this Component c, System.Action act ) { return c.gameObject.EventRelay().LifetimeValidator.Validated( act ); }


	[MethodImpl( AggrInline )] public static void LockSemaphore( this MonoBehaviour mb, ISemaphoreInterection semaphore, float seconds, object code )
	{
		mb.StartCoroutine( LockSemaphore( semaphore, seconds, code ) );
	}

	[MethodImpl( AggrInline )] static IEnumerator LockSemaphore( ISemaphoreInterection semaphore, float seconds, object code )
	{
		semaphore.Block( code );
		yield return new WaitForSeconds( seconds );
		semaphore.Release( code );
	}

	[MethodImpl( AggrInline )] public static void LockSemaphore( this MonoBehaviour mb, ISemaphoreInterection semaphore, float seconds, object code, string debug )
	{
		mb.StartCoroutine( LockSemaphoreDebuging( semaphore, seconds, code, debug ) );
	}

	static IEnumerator LockSemaphoreDebuging( ISemaphoreInterection semaphore, float seconds, object code, string debug )
	{
		// UnityEngine.Debug.LogFormat( "Blocking {0}{2} for {4}s with code {1} at {3}", debug, code.ToString(), semaphore, Time.time, seconds );
		semaphore.Block( code );
		yield return new WaitForSeconds( seconds );
		semaphore.Release( code );
		// UnityEngine.Debug.LogFormat( "Releasing {0}{2} with code {1} at {3}", debug, code.ToString(), semaphore, Time.time );
	}

	public static void FindSibling<T>( this Component c, ref T t ) where T : Component { if( c != null && t == null ) t = c.GetComponent<T>(); }
	public static void RequestSibling<T>( this Component c, ref T t ) where T : Component
	{
		FindSibling<T>( c, ref t );
		if( c != null && t == null ) t = c.gameObject.AddComponent<T>();
	}

	public static void FindSibling<T>( this GameObject go, ref T t ) where T : Component { if( go != null && t == null ) t = go.GetComponent<T>(); }
	public static void RequestSibling<T>( this GameObject go, ref T t ) where T : Component
	{
		FindSibling<T>( go, ref t );
		if( go != null && t == null ) t = go.AddComponent<T>();
	}

	public static T RequestSibling<T>( this GameObject go ) where T : Component
	{
		var find = go.GetComponent<T>();
		if( find != null ) return find;
		return go.AddComponent<T>();
	}

	public static void FindAncestry( this Component c, ref GameObject go, string name )
	{
		if( c == null || go != null ) return;
		var search = FindAncestry( c, name );
		if( search != null ) go = search.gameObject;
	}

	public static void FindAncestry( this Component c, ref Transform t, string name )
	{
		if( c == null || t != null ) return;
		t = FindAncestry( c, name );
	}

	public static Transform FindAncestry( this Component c, string name ) { if( c == null ) return null; return FindAncestry( c.transform, name ); }
	public static Transform FindAncestry( this GameObject go, string name ) { if( go == null ) return null; return FindAncestry( go.transform, name ); }
	public static Transform FindAncestry( this Transform t, string name )
	{
		if( t == null ) return null;
		var lowerName = name.ToLower();

		while( t != null )
		{
			if( t.name.ToLower() == lowerName ) return t;
			t = t.parent;
		}

		return null;
	}

	public static void FindAncestry<T>( this Component c, ref T t, string name ) where T : Component { if( c == null ) return; FindAncestry( c.transform, ref t, name ); }
	public static void FindAncestry<T>( this GameObject go, ref T t, string name ) where T : Component { if( go == null ) return; FindAncestry( go.transform, ref t, name ); }
	public static void FindAncestry<C>( this Transform t, ref C c, string name ) where C : Component
	{
		if( t == null || c != null ) return;
		var lowerName = name.ToLower();

		while( t != null )
		{
			if( t.name.ToLower() == lowerName ) { c = t.GetComponent<C>(); if( c != null ) return; }
			t = t.parent;
		}
	}

	public static void FindSiblingOrAncestry<T>( this Component c, ref T t ) where T : Component { FindSibling<T>( c, ref t ); FindAncestry<T>( c, ref t ); }
	public static void RequestSiblingOrAncestry<T>( this Component c, ref T t ) where T : Component { FindSibling<T>( c, ref t ); RequestAncestry<T>( c, ref t ); }

	public static void FindAncestry<T>( this Component c, ref T t ) where T : Component { if( c != null && t == null ) t = c.GetComponentInParent<T>(); }
	public static void RequestAncestry<T>( this Component c, ref T t ) where T : Component
	{
		FindAncestry<T>( c, ref t );
		if( c != null && t == null ) t = c.gameObject.AddComponent<T>();
	}

	public static void FindDescendent( this Component c, ref GameObject go, string name )
	{
		if( c == null || go != null ) return;
		var search = FindDescendent( c, name );
		if( search != null ) go = search.gameObject;
	}

	public static void FindDescendent( this Component c, ref Transform t, string name )
	{
		if( c == null || t != null ) return;
		t = FindDescendent( c, name );
	}

	public static Transform FindDescendent( this Component c, string name ) { if( c == null ) return null; return FindDescendent( c.transform, name ); }
	public static Transform FindDescendent( this GameObject go, string name ) { if( go == null ) return null; return FindDescendent( go.transform, name ); }
	public static Transform FindDescendent( this Transform transform, string name )
	{
		if( transform == null ) return null;

		List<Transform> list = new List<Transform> { transform };
		var lowerName = name.ToLower();

		while( list.Count > 0 )
		{
			var elemet = list[0];
			if( elemet.name.ToLower() == lowerName ) return elemet;

			list.RemoveAt( 0 );

			for( int i = 0; i < elemet.childCount; i++ )
				list.Add( elemet.GetChild( i ) );
		}

		return null;
	}

	public static void FindDescendent<T>( this Component c, ref T t, string name ) where T : Component { if( c == null ) return; FindDescendent( c.transform, ref t, name ); }
	public static void FindDescendent<T>( this GameObject go, ref T t, string name ) where T : Component { if( go == null ) return; FindDescendent( go.transform, ref t, name ); }
	public static void FindDescendent<T>( this Transform transform, ref T t, string name ) where T : Component
	{
		if( transform == null || t != null ) return;

		List<Transform> list = new List<Transform> { transform };
		if( string.IsNullOrEmpty( name ) )
		{
			transform.FindDescendent<T>( ref t );
			return;
		}
		var lowerName = name.ToLower();

		while( list.Count > 0 )
		{
			var elemet = list[0];
			if( elemet.name.ToLower() == lowerName )
			{
				t = elemet.GetComponent<T>();
				if( t != null ) return;
			}

			list.RemoveAt( 0 );

			for( int i = 0; i < elemet.childCount; i++ )
				list.Add( elemet.GetChild( i ) );
		}
	}

	public static void FindDescendent<T>( this Component c, ref T t ) where T : Component { if( c == null ) return; FindDescendent( c.gameObject, ref t ); }
	public static void FindDescendent<T>( this Transform transform, ref T t ) where T : Component { if( transform == null ) return; FindDescendent( transform.gameObject, ref t ); }
	public static void FindDescendent<T>( this GameObject go, ref T t ) where T : Component { if( go != null && t == null ) t = go.GetComponentInChildren<T>( true ); }
	public static void RequestDescendent<T>( this Component c, ref T t ) where T : Component { if( c == null ) return; RequestDescendent( c.gameObject, ref t ); }
	public static void RequestDescendent<T>( this Transform transform, ref T t ) where T : Component { if( transform == null ) return; RequestDescendent( transform.gameObject, ref t ); }
	public static void RequestDescendent<T>( this GameObject go, ref T t ) where T : Component
	{
		FindDescendent<T>( go, ref t );
		if( go != null && t == null ) t = go.AddComponent<T>();
	}

	[MethodImpl( AggrInline )] public static float CalculateArea( this Rect r ) { return r.width - r.height; }

	[MethodImpl( AggrInline )] public static float CalculateIntersectionArea( this Rect r, Rect b )
	{
		var topH = ( Mathf.Min( Mathf.Max( r.yMin, b.yMin ), r.yMax ) - r.yMin );
		var topArea = r.width * topH;
		var bottomY = Mathf.Min( r.yMax, Mathf.Max( r.yMin, b.yMax ) );
		var bottomH = ( r.yMax - bottomY );
		var bottomArea = r.width * bottomH;

		var sideY = Mathf.Max( r.yMin, b.yMin );
		var sideH = Mathf.Min( r.yMax, b.yMax ) - sideY;

		var leftW = ( Mathf.Min( Mathf.Max( r.xMin, b.xMin ), r.xMax ) - r.xMin );
		var leftArea = sideH * leftW;
		var rightX = Mathf.Min( r.xMax, Mathf.Max( r.xMin, b.xMax ) );
		var rightW = ( r.xMax - rightX );
		var rightArea = sideH * rightW;

		return ( r.CalculateArea() - ( topArea + bottomArea + leftArea + rightArea ) );
	}

	[MethodImpl( AggrInline )] public static Vector2 RandomPoint( this Rect r ) { return new Vector2( Random.Range( r.xMin, r.xMax ), Random.Range( r.yMin, r.yMax ) ); }
	[MethodImpl( AggrInline )] public static Vector2 RandomPoint( this Rect r, Rect exception )
	{
		var topH = ( Mathf.Min( Mathf.Max( r.yMin, exception.yMin ), r.yMax ) - r.yMin );
		var topArea = r.width * topH;
		var bottomY = Mathf.Min( r.yMax, Mathf.Max( r.yMin, exception.yMax ) );
		var bottomH = ( r.yMax - bottomY );
		var bottomArea = r.width * bottomH;

		var sideY = Mathf.Max( r.yMin, exception.yMin );
		var sideH = Mathf.Min( r.yMax, exception.yMax ) - sideY;

		var leftW = ( Mathf.Min( Mathf.Max( r.xMin, exception.xMin ), r.xMax ) - r.xMin );
		var leftArea = sideH * leftW;
		var rightX = Mathf.Min( r.xMax, Mathf.Max( r.xMin, exception.xMax ) );
		var rightW = ( r.xMax - rightX );
		var rightArea = sideH * rightW;

		var rnd = Random.Range( 0f, topArea + bottomArea + leftArea + rightArea );

		if( rnd < topArea ) return new Vector2( Random.Range( r.xMin, r.xMax ), Random.Range( r.yMin, r.yMin + topH ) );
		rnd -= topArea;
		if( rnd < bottomArea ) return new Vector2( Random.Range( r.xMin, r.xMax ), Random.Range( bottomY, r.yMax ) );
		rnd -= bottomArea;
		if( rnd < leftArea ) return new Vector2( Random.Range( r.xMin, r.xMin + leftW ), Random.Range( sideY, sideY + sideH ) );
		rnd -= leftArea;
		if( rnd < rightArea ) return new Vector2( Random.Range( rightX, rightX + rightW ), Random.Range( sideY, sideY + sideH ) );
		return Vector2.zero;
	}
	#endregion Rect

	#region Game Object
	[MethodImpl( AggrInline )] public static Bounds CalculateBounds( this GameObject go ) { return go.GetComponentsInChildren<Renderer>().CalculateBounds(); }
	#endregion Game Object

	#region Renderes
	[MethodImpl( AggrInline )] public static Bounds CalculateBounds( this IEnumerable<Renderer> renderers )
	{
		var min = Vector3.one * float.MaxValue;
		var max = Vector3.one * float.MinValue;

		int count = 0;
		foreach( var r in renderers )
		{
			count++;
			var bounds = r.bounds;
			min = Vector3.Min( min, bounds.min );
			max = Vector3.Max( max, bounds.max );
		}
		if( count == 0 ) min = max = Vector3.zero;

		return Algorithm.MinMaxBounds( min, max );
	}
	#endregion Renderes

	#region Colliders
	[MethodImpl( AggrInline )] public static void Cover( this BoxCollider box, IEnumerable<Renderer> renderers ) { box.Cover( renderers.CalculateBounds() ); }
	[MethodImpl( AggrInline )] public static void Cover( this BoxCollider box, Vector3 min, Vector3 max ) { box.Cover( Algorithm.MinMaxBounds( min, max ) ); }

	[MethodImpl( AggrInline )] public static void Cover( this BoxCollider box, Bounds bounds )
	{
		var size = bounds.size;

		if( !Mathf.Approximately( box.transform.lossyScale.x, 0 ) ) size.x /= box.transform.lossyScale.y;
		if( !Mathf.Approximately( box.transform.lossyScale.y, 0 ) ) size.y /= box.transform.lossyScale.y;
		if( !Mathf.Approximately( box.transform.lossyScale.z, 0 ) ) size.z /= box.transform.lossyScale.z;

		box.size = size;
		var center = bounds.center - box.transform.position;

		if( !Mathf.Approximately( box.transform.lossyScale.x, 0 ) ) center.x /= box.transform.lossyScale.y;
		if( !Mathf.Approximately( box.transform.lossyScale.y, 0 ) ) center.y /= box.transform.lossyScale.y;
		if( !Mathf.Approximately( box.transform.lossyScale.z, 0 ) ) center.z /= box.transform.lossyScale.z;

		box.center = center;
	}
	#endregion Colliders

	[MethodImpl( AggrInline )] public static string NameAndType( this Object obj, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name}<{obj.TypeNameOrNull()}>" : nullString;

	[MethodImpl( AggrInline )] public static string NameAndTypeColored( this Object obj, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name.Colorfy(Colors.Console.Names)}<{obj.TypeNameOrNullColored(Colors.Console.TypeName)}>" : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( AggrInline )] public static string NameAndTypeColored( this Object obj, Color nameColor, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(Colors.Console.TypeName)}>" : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( AggrInline )] public static string NameAndTypeColored( this Object obj, Color nameColor, Color typeColor, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(typeColor)}>" : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( AggrInline )] public static string NameAndTypeColored( this Object obj, Color nameColor, Color typeColor, Color nullColor, string nullString = ConstsK10.NULL_STRING )=> ( obj != null ) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(typeColor)}>" : nullString.Colorfy(nullColor);

	[MethodImpl( AggrInline )] public static string NameOrNull( this Object obj, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.name : nullString;
	[MethodImpl(AggrInline)] public static string DebugNameOrNull(this object obj, string nullString = ConstsK10.NULL_STRING)
	{
		if (obj == null) return nullString;
		if (obj is IDebugName dName) return dName.DebugName;
		var uObj = obj as Object;
		return uObj.NameOrNull(nullString);
	}
	[MethodImpl(AggrInline)] public static string ToStringColored( this bool boolValue ) => boolValue.ToString().Colorfy( boolValue ? Colors.Console.Numbers : Colors.Console.Negation );
	[MethodImpl( AggrInline )] public static string ToStringColored( this object obj, Color valueColor ) => obj.ToString().Colorfy(valueColor);
    [MethodImpl(AggrInline)]
    public static string ToStringOrNull(this object obj, string nullString = ConstsK10.NULL_STRING)
    {
		if( obj == null ) return nullString;
		if (obj is IEnumerable enumerable)
		{
			var count = "...";
			if (obj is ICollection collection) count = collection.Count.ToString();
			var sb = StringBuilderPool.RequestWith($"<{obj.TypeNameOrNull()}>[{count}]{{ ");
			bool first = true;
			foreach (var e in enumerable)
			{
				if( !first ) sb.Append(", ");
				first = false;
				sb.Append(e.ToStringOrNull());
			}
			sb.Append( " }}" );
			return sb.ReturnToPoolAndCast();
		}
        return obj.ToString();
    }

    [MethodImpl( AggrInline )] public static string ToStringOrNullColored( this object obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.ToString().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( AggrInline )] public static string ToStringOrNullColored( this object obj, Color valueColor, Color nullColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.ToString().Colorfy(valueColor) : nullString.Colorfy(nullColor);
	[MethodImpl( AggrInline )] public static string HierarchyNameOrNull( this GameObject obj, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName() : nullString;
	[MethodImpl( AggrInline )] public static string HierarchyNameOrNullColored( this GameObject obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( AggrInline )] public static string HierarchyNameOrNull( this Transform obj, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName() : nullString;
	[MethodImpl( AggrInline )] public static string HierarchyNameOrNullColored( this Transform obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( AggrInline )] public static string HierarchyNameOrNull( this Component obj, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ? ( obj.transform.HierarchyName() + $"<{( obj != null ? obj.GetType().ToString() : nullString )}>" ) : nullString );
	[MethodImpl( AggrInline )] public static string HierarchyNameOrNullColored( this Component obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ? ( obj.transform.HierarchyName().Colorfy(valueColor) + $"<{( obj != null ? obj.GetType().ToString() : nullString.Colorfy(Colors.Console.Negation) )}>" ) : nullString.Colorfy(Colors.Console.Negation) );
}
