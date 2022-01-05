
using System.Runtime.CompilerServices;
using UnityEngine;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v2 = Unity.Mathematics.float2;
using v3 = Unity.Mathematics.float3;
#else
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;
#endif

public static class MathAdapter
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;

#if USE_NEW_MATHEMATICS
	public const float PI = math.PI;
#else
	public const float PI = Mathf.PI;
#endif

#if USE_NEW_MATHEMATICS
	[MethodImpl( AggrInline )] public static float2 IgnoreY( this float3 v3 ) { return new float2( v3.x, v3.z ); }
	[MethodImpl( AggrInline )] public static float2 IgnoreZ( this float3 v3 ) { return new float2( v3.x, v3.y ); }
	[MethodImpl( AggrInline )] public static float3 WithZ( this float2 v2, float zValue ) { return new float3( v2.x, v2.y, zValue ); }
	[MethodImpl( AggrInline )] public static float3 WithZ0( this float2 v2 ) { return new float3( v2.x, v2.y, 0 ); }
#endif

#if USE_NEW_MATHEMATICS
	[MethodImpl( AggrInline )] public static float abs( float a ) => math.abs( a );
	[MethodImpl( AggrInline )] public static float min( float a, float b ) => math.min( a, b );
	[MethodImpl( AggrInline )] public static float max( float a, float b ) => math.max( a, b );
	[MethodImpl( AggrInline )] public static float sqrt( float a ) => math.sqrt( a );
	[MethodImpl( AggrInline )] public static float sin( float a ) => math.sin( a );
	[MethodImpl( AggrInline )] public static float cos( float a ) => math.cos( a );
	[MethodImpl( AggrInline )] public static float acos( float a ) => math.acos( a );
	[MethodImpl( AggrInline )] public static float atan2( float y, float x ) => math.atan2( y, x );
	[MethodImpl( AggrInline )] public static int CeilToInt( float a ) => Mathf.CeilToInt( a ); // TODO: Change to New Mathematics
	[MethodImpl( AggrInline )] public static int RoundToInt( float a ) => Mathf.RoundToInt( a ); // TODO: Change to New Mathematics
#else
	[MethodImpl( AggrInline )] public static float abs( float a ) => Mathf.Abs( a );
	[MethodImpl( AggrInline )] public static float min( float a, float b ) => Mathf.Min( a, b );
	[MethodImpl( AggrInline )] public static float max( float a, float b ) => Mathf.Max( a, b );
	[MethodImpl( AggrInline )] public static float sqrt( float a ) => Mathf.Sqrt( a );
	[MethodImpl( AggrInline )] public static float sin( float a ) => Mathf.Sin( a );
	[MethodImpl( AggrInline )] public static float cos( float a ) => Mathf.Cos( a );
	[MethodImpl( AggrInline )] public static float acos( float a ) => Mathf.acos( a );
	[MethodImpl( AggrInline )] public static float atan2( float y, float x ) => Mathf.Atan2( y, x );
	[MethodImpl( AggrInline )] public static int CeilToInt( float a ) => Mathf.CeilToInt( a );
	[MethodImpl( AggrInline )] public static int RoundToInt( float a ) => Mathf.RoundToInt( a );
#endif

	//Vector3
#if USE_NEW_MATHEMATICS
	[MethodImpl( AggrInline )] public static float dot( v3 a, v3 b ) => math.dot( a, b );
	[MethodImpl( AggrInline )] public static v3 cross( v3 a, v3 b ) => math.cross( a, b );
	[MethodImpl( AggrInline )] public static v3 normalize( v3 a ) => math.normalize( a );
	[MethodImpl( AggrInline )] public static float length( v3 a ) => math.length( a );
	[MethodImpl( AggrInline )] public static float lengthsq( v3 a ) => math.lengthsq( a );
	[MethodImpl( AggrInline )] public static v3 compMul( v3 a, v3 b ) => a * b;
#else
	[MethodImpl( AggrInline )] public static float dot( v3 a, v3 b ) => Vector3.Dot( a, b );
	[MethodImpl( AggrInline )] public static v3 cross( v3 a, v3 b ) => Vector3.Cross( a, b );
	[MethodImpl( AggrInline )] public static v3 normalize( v3 a ) => Vector3.Normalize( a );
	[MethodImpl( AggrInline )] public static float length( v3 a ) => Vector3.Magnitude( a );
	[MethodImpl( AggrInline )] public static float lengthsq( v3 a ) => Vector3.SqrMagnitude( a );
	[MethodImpl( AggrInline )] public static v3 compMul( v3 a, v3 b ) => Vector3.Scale( a, b );
#endif

	//Vector2
#if USE_NEW_MATHEMATICS
	[MethodImpl( AggrInline )] public static float dot( v2 a, v2 b ) => math.dot( a, b );
	[MethodImpl( AggrInline )] public static v2 normalize( v2 a ) => math.normalize( a );
	[MethodImpl( AggrInline )] public static float length( v2 a ) => math.length( a );
	[MethodImpl( AggrInline )] public static float lengthsq( v2 a ) => math.lengthsq( a );
	[MethodImpl( AggrInline )] public static v2 compMul( v2 a, v2 b ) => a * b;
#else
	[MethodImpl( AggrInline )] public static float dot( v2 a, v2 b ) => Vector2.Dot( a, b );
	[MethodImpl( AggrInline )] public static v2 normalize( v2 a ) => Vector2.Normalize( a );
	[MethodImpl( AggrInline )] public static float length( v2 a ) => Vector2.Magnitude( a );
	[MethodImpl( AggrInline )] public static float lengthsq( v2 a ) => Vector2.SqrMagnitude( a );
	[MethodImpl( AggrInline )] public static v2 compMul( v2 a, v2 b ) => Vector2.Scale( a, b );
#endif
}