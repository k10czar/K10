
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v2 = Unity.Mathematics.float2;
using v3 = Unity.Mathematics.float3;
using m44 = Unity.Mathematics.float4x4;
#else
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;
using m44 = UnityEngine.Matrix4x4;
#endif

public static class MathAdapter
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;
	public const float EP2 = float.Epsilon * 2;

#if USE_NEW_MATHEMATICS
	public const float PI = math.PI;
#else
	public const float PI = Mathf.PI;
#endif

	public const float RadiansToDegrees = 180f / PI;
	public const float DegreesToRadians = 1 / RadiansToDegrees;

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
	[MethodImpl( AggrInline )] public static float tan( float a ) => math.tan( a );
	[MethodImpl( AggrInline )] public static float atan2( float y, float x ) => math.atan2( y, x );
	[MethodImpl( AggrInline )] public static float smoothStep(float a, float b, float x) => math.smoothStep( a, b, x );
	[MethodImpl( AggrInline )] public static int CeilToInt( float a ) => Mathf.CeilToInt( a ); // TODO: Change to New Mathematics
	[MethodImpl( AggrInline )] public static int RoundToInt( float a ) => Mathf.RoundToInt( a ); // TODO: Change to New Mathematics
	[MethodImpl( AggrInline )] public static bool Approximately(float a, float b) => math.abs( a - b ) < EP2;
#else
	[MethodImpl( AggrInline )] public static float abs( float a ) => Mathf.Abs( a );
	[MethodImpl( AggrInline )] public static float min( float a, float b ) => Mathf.Min( a, b );
	[MethodImpl( AggrInline )] public static float max( float a, float b ) => Mathf.Max( a, b );
	[MethodImpl( AggrInline )] public static float sqrt( float a ) => Mathf.Sqrt( a );
	[MethodImpl( AggrInline )] public static float sin( float a ) => Mathf.Sin( a );
	[MethodImpl( AggrInline )] public static float cos( float a ) => Mathf.Cos( a );
	[MethodImpl( AggrInline )] public static float acos( float a ) => Mathf.Acos( a );
	[MethodImpl( AggrInline )] public static float tan( float a ) => Mathf.Tan( a );
	[MethodImpl( AggrInline )] public static float atan2( float y, float x ) => Mathf.Atan2( y, x );
	[MethodImpl( AggrInline )] public static float smoothStep(float a, float b, float x) => Mathf.SmoothStep( a, b, x );
	[MethodImpl( AggrInline )] public static int CeilToInt( float a ) => Mathf.CeilToInt( a );
	[MethodImpl( AggrInline )] public static int RoundToInt( float a ) => Mathf.RoundToInt( a );
	[MethodImpl(AggrInline)] public static bool Approximately(float a, float b) => Mathf.Abs(a - b) < EP2;
	// [MethodImpl(AggrInline)] public static float sign( float a ) => Mathf.Sign( a );
#endif
	[MethodImpl( AggrInline )] public static int sign( float x ) => x < 0 ? -1 : 1;

	//Vector3
#if USE_NEW_MATHEMATICS
	[MethodImpl( AggrInline )] public static float dot( v3 a, v3 b ) => math.dot( a, b );
	[MethodImpl( AggrInline )] public static v3 cross( v3 a, v3 b ) => math.cross( a, b );
	[MethodImpl( AggrInline )] public static v3 normalize( v3 a ) => math.normalize( a );
	[MethodImpl( AggrInline )] public static float length( v3 a ) => math.length( a );
	[MethodImpl( AggrInline )] public static float lengthsq( v3 a ) => math.lengthsq( a );
	[MethodImpl( AggrInline )] public static v3 compMul( v3 a, v3 b ) => a * b;
	[MethodImpl( AggrInline )] public static v3 max( v3 a, v3 b ) => math.max( a, b );
	[MethodImpl( AggrInline )] public static v3 min( v3 a, v3 b ) => math.min( a, b );
#else
	[MethodImpl( AggrInline )] public static float dot( v3 a, v3 b ) => Vector3.Dot( a, b );
	[MethodImpl( AggrInline )] public static v3 cross( v3 a, v3 b ) => Vector3.Cross( a, b );
	[MethodImpl( AggrInline )] public static v3 normalize( v3 a ) => Vector3.Normalize( a );
	[MethodImpl( AggrInline )] public static float length( v3 a ) => Vector3.Magnitude( a );
	[MethodImpl( AggrInline )] public static float lengthsq( v3 a ) => Vector3.SqrMagnitude( a );
	[MethodImpl( AggrInline )] public static v3 compMul( v3 a, v3 b ) => Vector3.Scale( a, b );
	[MethodImpl( AggrInline )] public static v3 max( v3 a, v3 b ) => Vector3.Max( a, b );
	[MethodImpl( AggrInline )] public static v3 min( v3 a, v3 b ) => Vector3.Min( a, b );
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
	[MethodImpl( AggrInline )] public static v2 normalize( v2 a ) => a.normalized;
	[MethodImpl( AggrInline )] public static float length( v2 a ) => a.magnitude;
	[MethodImpl( AggrInline )] public static float lengthsq( v2 a ) => a.sqrMagnitude;
	[MethodImpl( AggrInline )] public static v2 compMul( v2 a, v2 b ) => Vector2.Scale( a, b );
#endif

#if USE_NEW_MATHEMATICS
	// [MethodImpl( AggrInline )] public static m44 mul( m44 a, m44 b ) => math.mul( a, b );
#else
	// [MethodImpl(AggrInline)] public static m44 mul(m44 a, m44 b) => a * b;
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static v3 To3d( this v2 v2d, v3 origin, v3 v3dAxisX, v3 v3dAxisY) => origin + v2d.x * v3dAxisX + v2d.y * v3dAxisY;


	[MethodImpl(AggrInline)]
	public static void MoveAway( ref v3 a, ref v3 b, float percentage )
	{
		var delta = a - b;
		var interaction = delta * percentage * .5f;
		a += interaction;
		b -= interaction;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Bhaskara(float a, float b, float c, out float root1, out float root2)
	{
		var delta = b * b - 4 * a * c;
		var deltaRoot = sqrt(delta);
		var bMinus = -b;
		var a2 = 2 * a;
		root1 = (bMinus + deltaRoot) / a2;
		root2 = (bMinus - deltaRoot) / a2;
	}
}