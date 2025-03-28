using UnityEngine;

namespace  K10.Interpolation.Funcs
{
    public class Vector2Lerp : IInterpolationFunc<Vector2>
    {
        public Vector2 Interpolate( Vector2 a, Vector2 b, float delta )
        {
            return new Vector2( MathAdapter.lerp( a.x, b.x, delta ),
                                MathAdapter.lerp( a.y, b.y, delta ) );
        }
    }
}
