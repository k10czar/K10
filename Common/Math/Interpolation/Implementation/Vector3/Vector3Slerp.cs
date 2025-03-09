using UnityEngine;

namespace  K10.Interpolation.Funcs
{
    public class Vector3Slerp : IInterpolationFunc<Vector3>
    {
        public Vector3 Interpolate(Vector3 a, Vector3 b, float delta)
        {
            return Vector3.SlerpUnclamped(a, b, delta);
        }
    }
}
