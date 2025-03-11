namespace  K10.Interpolation.Funcs
{
    public class FloatDegreesLerp : IInterpolationFunc<float>
    {
        public float Interpolate( float a, float b, float delta ) => MathAdapter.degreesLerp( a, b, delta );
    }
}
