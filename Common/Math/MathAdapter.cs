#if TRY_USE_NEW_MATH && !DO_NOT_USE_NEW_MATH //USE_NEW_MATHEMATICS
#define USE_NEW_MATHEMATICS
#endif
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v2 = Unity.Mathematics.float2;
using v3 = Unity.Mathematics.float3;
using v4 = Unity.Mathematics.float4;
// using m33 = Unity.Mathematics.float3x3;
// using m44 = Unity.Mathematics.float4x4;
#else
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;
using v4 = UnityEngine.Vector4;
// using m33 = UnityEngine.Matrix3x3;
// using m44 = UnityEngine.Matrix4x4;
#endif

public static class MathAdapter
{
	public const float EP2 = float.Epsilon * 2;
	public const float NEG_EP2 = -EP2;
	
	public static readonly v3 right = new v3( 1, 0, 0 );
	public static readonly v3 left = new v3( -1, 0, 0 );
	public static readonly v3 up = new v3( 0, 1, 0 );
	public static readonly v3 down = new v3( 0, -1, 0 );
	public static readonly v3 forward = new v3( 0, 0, 1 );
	public static readonly v3 back = new v3( 0, 0, -1 );
	
    const float DEGREES_MIN = -180;
    const float DEGREES_MAX = 180;
    const float DEGREES_DELTA = DEGREES_MAX - DEGREES_MIN;
	
    const float RADIANS_MIN = -MathAdapter.PI;
    const float RADIANS_MAX = MathAdapter.PI;
    const float RADIANS_DELTA = RADIANS_MAX - RADIANS_MIN;

#if USE_NEW_MATHEMATICS
	public const float PI = math.PI;
#else
	public const float PI = Mathf.PI;
#endif
	public const float PI2 = 2*PI;

	public const float RadiansToDegrees = 180f / PI;
	public const float DegreesToRadians = 1 / RadiansToDegrees;

#if USE_NEW_MATHEMATICS
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float2 IgnoreY( this float3 v3 ) { return new float2( v3.x, v3.z ); }
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float2 IgnoreZ( this float3 v3 ) { return new float2( v3.x, v3.y ); }
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float3 WithZ( this float2 v2, float zValue ) { return new float3( v2.x, v2.y, zValue ); }
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float3 WithZ0( this float2 v2 ) { return new float3( v2.x, v2.y, 0 ); }
#endif

#if USE_NEW_MATHEMATICS
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float abs( float a ) => math.abs( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float min( float a, float b ) => math.min( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float max( float a, float b ) => math.max( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float sqrt( float a ) => math.sqrt( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float sin( float a ) => math.sin( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float cos( float a ) => math.cos( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float acos( float a ) => math.acos( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float tan( float a ) => math.tan( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float atan2( float y, float x ) => math.atan2( y, x );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float smoothStep(float a, float b, float x) => math.smoothstep( a, b, x );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static int CeilToInt( float a ) => Mathf.CeilToInt( a ); // TODO: Change to New Mathematics
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static int RoundToInt( float a ) => Mathf.RoundToInt( a ); // TODO: Change to New Mathematics
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static bool Approximately(float a, float b) => math.abs( a - b ) < EP2;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static bool Approximately(float a, float b, float tolerance) => math.abs( a - b ) < tolerance;
#else
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float abs( float a ) => Mathf.Abs( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float min( float a, float b ) => Mathf.Min( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float max( float a, float b ) => Mathf.Max( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float sqrt( float a ) => Mathf.Sqrt( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float sin( float a ) => Mathf.Sin( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float cos( float a ) => Mathf.Cos( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float acos( float a ) => Mathf.Acos( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float tan( float a ) => Mathf.Tan( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float atan2( float y, float x ) => Mathf.Atan2( y, x );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float smoothStep(float a, float b, float x) => Mathf.SmoothStep( a, b, x );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static int CeilToInt( float a ) => Mathf.CeilToInt( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static int RoundToInt( float a ) => Mathf.RoundToInt( a );
	[MethodImpl(Optimizations.INLINE_IF_CAN)] public static bool Approximately(float a, float b) => Mathf.Abs(a - b) < EP2;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static bool Approximately(float a, float b, float tolerance) => Mathf.Abs( a - b ) < tolerance;
	// [MethodImpl(Optimizations.INLINE_IF_CAN)] public static float sign( float a ) => Mathf.Sign( a );
#endif
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static int sign( float x ) => x < 0 ? -1 : 1;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float clamp( float val, float min, float max ) => val < min ? min : ( val > max ? max : val );

	//Vector3
#if USE_NEW_MATHEMATICS
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float dot( v3 a, v3 b ) => math.dot( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 cross( v3 a, v3 b ) => math.cross( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 normalize( v3 a ) => math.normalize( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float length( v3 a ) => math.length( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float lengthsq( v3 a ) => math.lengthsq( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 compMul( v3 a, v3 b ) => a * b;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 max( v3 a, v3 b ) => math.max( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 min( v3 a, v3 b ) => math.min( a, b );
#else
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float dot( v3 a, v3 b ) => Vector3.Dot( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 cross( v3 a, v3 b ) => Vector3.Cross( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 normalize( v3 a ) => Vector3.Normalize( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float length( v3 a ) => Vector3.Magnitude( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float lengthsq( v3 a ) => Vector3.SqrMagnitude( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 compMul( v3 a, v3 b ) => Vector3.Scale( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 max( v3 a, v3 b ) => Vector3.Max( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v3 min( v3 a, v3 b ) => Vector3.Min( a, b );
#endif

	//Vector2
#if USE_NEW_MATHEMATICS
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float dot( v2 a, v2 b ) => math.dot( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v2 normalize( v2 a ) => math.normalize( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float length( v2 a ) => math.length( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float lengthsq( v2 a ) => math.lengthsq( a );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v2 compMul( v2 a, v2 b ) => a * b;
#else
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float dot( v2 a, v2 b ) => Vector2.Dot( a, b );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v2 normalize( v2 a ) => a.normalized;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float length( v2 a ) => a.magnitude;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float lengthsq( v2 a ) => a.sqrMagnitude;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static v2 compMul( v2 a, v2 b ) => Vector2.Scale( a, b );
#endif

#if USE_NEW_MATHEMATICS
	// [MethodImpl( Optimizations.INLINE_IF_CAN )] public static m44 mul( m44 a, m44 b ) => math.mul( a, b );
#else
	// [MethodImpl(Optimizations.INLINE_IF_CAN)] public static m44 mul(m44 a, m44 b) => a * b;
#endif

	[MethodImpl( Optimizations.INLINE_IF_CAN )]
    public static float lerp(float a, float b, float delta )
    {
        var ab = b - a;
        return a + ( ab * delta );
    }

	[MethodImpl( Optimizations.INLINE_IF_CAN )]
    public static float circularLerp(float a, float b, float delta, float min, float range)
    {
        var ab = b - a;
        var val = a + ( ab * delta );
        if( val < min || val - min > range ) val = ( ( val - min ) % range ) + min;
        return val;
    }

	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float degreesLerp(float a, float b, float delta ) => circularLerp( a, b, delta, DEGREES_MIN, DEGREES_DELTA );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static float radiansLerp(float a, float b, float delta ) => circularLerp( a, b, delta, RADIANS_MIN, RADIANS_DELTA );

	[MethodImpl( Optimizations.INLINE_IF_CAN )]
	public static v3 To3d( this v2 v2d, v3 origin, v3 v3dAxisX, v3 v3dAxisY) => origin + v2d.x * v3dAxisX + v2d.y * v3dAxisY;


	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static void MoveAway( ref v3 a, ref v3 b, float percentage )
	{
		var delta = a - b;
		var interaction = delta * percentage * .5f;
		a += interaction;
		b -= interaction;
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
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