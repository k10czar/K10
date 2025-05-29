using UnityEngine;
using System.Runtime.CompilerServices;

public static class VectorExtensions
{
    [MethodImpl( Optimizations.INLINE_IF_CAN )]
	public static Vector3 OnLineSegment( this Vector3 point, Vector3 from, Vector3 to )
	{
		var delta = to - from;
		var mag = delta.magnitude;
		var t = point - from;
		var nDir = delta / mag;
		var dot = Vector3.Dot( nDir, t );
		return from + nDir * Mathf.Clamp( dot, 0, mag );
	}

    [MethodImpl( Optimizations.INLINE_IF_CAN )]
    public static Vector3 GetHasPolynomialResult( this Vector3[] terms, float x )
    {
        var v = Vector3.zero;
        var exp = 1f;
        for( int i = 0; i < terms.Length; i++ )
        {
            var t = terms[i];
            v += t * exp;
            exp *= x;
        }
        return v;
    }

    [MethodImpl( Optimizations.INLINE_IF_CAN )]
    public static bool IsCloser( this Vector3 a, Vector3 b, float dimensionTolerance = float.Epsilon )
    {
        var dx = b.x - a.x;
        if (dx < dimensionTolerance && dx > -dimensionTolerance)
        {
            var dz = b.z - a.z;
            if (dz < dimensionTolerance && dz > -dimensionTolerance)
            {
                var dy = b.y - a.y;
                if (dy < dimensionTolerance && dy > -dimensionTolerance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl( Optimizations.INLINE_IF_CAN )]
    public static Vector3 RandomShake( this Vector3 v, float maxMag )
    {
        var randonOffsetMag = UnityEngine.Random.Range( 0, maxMag );
        var randonAngZ = UnityEngine.Random.Range( 0, MathAdapter.PI2 );
        var randonAngY = UnityEngine.Random.Range( 0, MathAdapter.PI2 );
        return Shake( v, randonOffsetMag, randonAngZ, randonAngY );
    }

    [MethodImpl( Optimizations.INLINE_IF_CAN )]
    public static Vector3 Shake( this Vector3 v, float mag, float angleZ, float angleY )
    {        
        var cosZ = MathAdapter.cos( angleZ );
        var sinZ = MathAdapter.sin( angleZ );
        var cosY = MathAdapter.cos( angleY );
        var sinY = MathAdapter.sin( angleY );

        var offX = mag * cosY * cosZ;
        var offY = mag * cosY * sinZ;
        var offZ = mag * -sinY;

        return new Vector3( v.x + offX, v.y + offY, v.z + offZ );
    }
}