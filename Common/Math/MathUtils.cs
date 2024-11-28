using UnityEngine;
using System.Runtime.CompilerServices;

public static class MathUtils
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;

	[MethodImpl( AggrInline )] public static Vector2 IgnoreY( this Vector3 v3 ) { return new Vector2( v3.x, v3.z ); }
	[MethodImpl( AggrInline )] public static Vector2 IgnoreZ( this Vector3 v3 ) { return new Vector2( v3.x, v3.y ); }
	[MethodImpl( AggrInline )] public static Vector3 WithZ( this Vector2 v2, float zValue ) { return new Vector3( v2.x, v2.y, zValue ); }
	[MethodImpl( AggrInline )] public static Vector3 WithZ0( this Vector2 v2 ) { return new Vector3( v2.x, v2.y, 0 ); }
	
	[MethodImpl( AggrInline )] public static Vector3 WithX( this Vector3 v3, float xValue ) { return new Vector3( xValue, v3.y, v3.z ); }
	[MethodImpl( AggrInline )] public static Vector3 WithY( this Vector3 v3, float yValue ) { return new Vector3( v3.x, yValue, v3.z ); }
	[MethodImpl( AggrInline )] public static Vector3 WithZ( this Vector3 v3, float zValue ) { return new Vector3( v3.x, v3.y, zValue ); }
}