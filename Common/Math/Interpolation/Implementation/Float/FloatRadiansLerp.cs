namespace  K10.Interpolation.Funcs
{
    public class FloatRadiansLerp : IInterpolationFunc<float>
    {
        public float Interpolate( float a, float b, float delta ) => MathAdapter.radiansLerp( a, b, delta );
    }
}
