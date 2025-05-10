using UnityEngine;

namespace  K10.Interpolation.Funcs
{
    public class Vector3Lerp : IInterpolationFunc<Vector3>
    {
        public Vector3 Interpolate(Vector3 a, Vector3 b, float delta)
        {
            return new Vector3( MathAdapter.lerp( a.x, b.x, delta ),
                                MathAdapter.lerp( a.y, b.y, delta ),
                                MathAdapter.lerp( a.z, b.z, delta ) );
        }
    }
}
