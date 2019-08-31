using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EVerticalAlign { Top, Center, Bottom }

namespace K10
{
    namespace Utils
    {
        namespace Unity
		{
            public struct Algorithm
			{
				static List<float> _sizesCache = new List<float>();
				static List<float> _offsetCache = new List<float>();

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

				public static void UpdateChildrenOrganizers( GameObject go )
				{
					var organizers = go.GetComponentsInChildren<BaseOrganizer>( true );
					for( int i = organizers.Length -1; i >= 0; i-- ) organizers[ i ].UpdateOrganization();
				}

				public static void UpdateParentOrganizers( GameObject go )
				{
					var organizers = go.GetComponentsInParent<BaseOrganizer>( true );
					for( int i = 0; i < organizers.Length; i++ ) organizers[ i ].UpdateOrganization();
				}

				public static void HorizontalOrganizer( Transform transform, float spacing = .05f, TextAlignment align = TextAlignment.Center, bool countInactive = true, List<Transform> ignoreds = null )
				{
//					Debug.Log( "HorizontalOrganizer on " + transform.name );
					var count = transform.childCount;
					var totalSize = 0f;

					while( count > _sizesCache.Count ) _sizesCache.Add( 0 );
					while( count > _offsetCache.Count ) _offsetCache.Add( 0 );

					var scale = transform.lossyScale.x;
					spacing *= scale;

					if( Mathf.Approximately( scale, 0 ) ) scale = .0000001f;
					var realCount = 0;

					for( int i = 0; i < count; i++ )
					{
						var child = transform.GetChild( i );
						if( !countInactive && !child.gameObject.activeInHierarchy ) continue;

						realCount++;

						var min = float.MaxValue;
						var max = float.MinValue;
						bool visible = false;

						foreach( var rend in child.GetComponentsInChildren<Renderer>() )
						{
							if( ChildOfSomeTransform( rend, ignoreds ) )
								continue;
							
							var bounds = rend.bounds;
							min = Mathf.Min( min, bounds.min.x );
							max = Mathf.Max( max, bounds.max.x );
							visible = true;
						}

						var size = visible ? ( max - min ) : 0f;

						_sizesCache[ i ] = size;
						_offsetCache[ i ] = visible ? ( child.position.x - min ) : 0;

						totalSize += _sizesCache[ i ];
					}

					for( int i = 0; i < count; i++ )
					{
						var child = transform.GetChild( i );
						if( !countInactive && !child.gameObject.activeInHierarchy )
							continue;

						var pos = child.localPosition;
						pos.y = 0;
						child.localPosition = pos;
					}
						
					totalSize += ( realCount - 1 ) * spacing;
					var hs = totalSize / 2;
					var it = -hs;

					switch( align )
					{
						case TextAlignment.Left: it = 0; break;
						case TextAlignment.Center: it = -hs; break;
						case TextAlignment.Right: it = -totalSize; break;
					}

					Debug.DrawLine( transform.position + ( Vector3.right * it ) + Vector3.up * .1f, transform.position + ( Vector3.right * ( it + totalSize ) ) + Vector3.up * .1f, Color.red );

					while( count > _sizesCache.Count ) _sizesCache.Add( 0 );

					for( int i = 0; i < count; i++ )
					{
						var child = transform.GetChild( i );
						if( !countInactive && !child.gameObject.activeInHierarchy )
							continue;

						var pos = child.localPosition;
						pos.x = ( it + _offsetCache[i] ) / scale;

						child.localPosition = pos;
						it += _sizesCache[ i ] + spacing;
					}
				}

				public static bool ChildOfSomeTransform( Component comp, List<Transform> ignoreds = null ) { if( comp == null ) return false; return ChildOfSomeTransform( comp.transform, ignoreds ); }
				public static bool ChildOfSomeTransform( GameObject go, List<Transform> ignoreds = null ) { if( go == null ) return false; return ChildOfSomeTransform( go.transform, ignoreds ); }

				public static bool ChildOfSomeTransform( Transform transform, List<Transform> ignoreds = null )
				{
					if( transform == null || ignoreds == null )
						return false;
					
					for( int ig = 0; ig < ignoreds.Count; ig++ )
					{
						var t = ignoreds[ ig ];
						if( t != null && ( transform == t || transform.IsChildOf( t ) ) )
							return true;
					}

					return false;
				}

				public static void VerticalOrganizer( Transform transform, float spacing = .1f, EVerticalAlign align = EVerticalAlign.Center, bool countInactive = true, List<Transform> ignoreds = null )
				{
					var count = transform.childCount;
					var totalSize = 0f;
					var realCount = 0;
				
					while( count > _sizesCache.Count ) _sizesCache.Add( 0 );
					while( count > _offsetCache.Count ) _offsetCache.Add( 0 );

					var scale = transform.lossyScale.y;
					spacing *= scale;

					for( int i = 0; i < count; i++ )
					{
						var child = transform.GetChild( i );
						if( !countInactive && !child.gameObject.activeInHierarchy ) continue;

						realCount++;

						var min = float.MaxValue;
						var max = float.MinValue;
						bool visible = false;

						foreach( var rend in child.GetComponentsInChildren<Renderer>() )
						{
							if( ChildOfSomeTransform( rend, ignoreds ) )
								continue;
							
							var bounds = rend.bounds;
							min = Mathf.Min( min, bounds.min.y );
							max = Mathf.Max( max, bounds.max.y );
							visible = true;
						}

						var size = visible ? ( max - min ) : 0f;

						_sizesCache[ i ] = size;
						_offsetCache[ i ] = visible ? ( child.position.y - min ) : 0;

						totalSize += size;
					}

					for( int i = 0; i < count; i++ )
					{
						var child = transform.GetChild( i );
						if( !countInactive && !child.gameObject.activeInHierarchy )
							continue;
						var pos = child.localPosition;
						pos.x = 0;
						child.localPosition = pos;
					}

					totalSize += ( realCount - 1 ) * spacing;
					var hs = totalSize / 2;
					var it = -hs;

					switch( align )
					{
						case EVerticalAlign.Top: it = -totalSize; break;
						case EVerticalAlign.Center: it = -hs; break;
						case EVerticalAlign.Bottom: it = 0; break;
					}

					for( int i = count - 1; i >= 0; i-- )
					{
						var child = transform.GetChild( i );

						if( !countInactive && !child.gameObject.activeInHierarchy )
							continue;

						var pos = child.localPosition;
						pos.y = ( it + _offsetCache[i] ) / scale;
						child.localPosition = pos;
						it += _sizesCache[ i ] + spacing;
					}
				}
            }
        }
    }
}
