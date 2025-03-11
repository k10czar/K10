namespace  K10.Interpolation.Funcs
{
    public class FloatLerp : IInterpolationFunc<float>
    {
        public float Interpolate( float a, float b, float delta ) => MathAdapter.lerp( a, b, delta );
    }
}
