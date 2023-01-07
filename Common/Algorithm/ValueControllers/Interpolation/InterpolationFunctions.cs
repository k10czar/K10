using UnityEngine;

public interface IInterpolationFunction
{
	float Evaluate( float normilizedTime );
}

public enum ENormalizedValueInterpolation { Linear, Quadratic, SquareRoot, Cubic, CubeRoot, SmoothStep, Sine }

public static class InterpolationExtensions
{
	public static IInterpolationFunction GetFunction( this ENormalizedValueInterpolation type )
	{
		switch( type )
		{
			case ENormalizedValueInterpolation.Linear: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.Quadratic: return PowerInterpolation.Pow2;
			case ENormalizedValueInterpolation.SquareRoot: return PowerInterpolation.Sqrt;
			case ENormalizedValueInterpolation.Cubic: return PowerInterpolation.Pow3;
			case ENormalizedValueInterpolation.CubeRoot: return PowerInterpolation.CubeRoot;
			case ENormalizedValueInterpolation.SmoothStep: return SmoothStepInterpolation.Instance;
			case ENormalizedValueInterpolation.Sine: return SineInterpolation.Instance;
		}
		return LinearInterpolation.Instance;
	}
}

public class LinearInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction Instance = new LinearInterpolation();
	private LinearInterpolation() { }
	public float Evaluate( float normilizedDuration ) => normilizedDuration;
}

public class SmoothStepInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction Instance = new SmoothStepInterpolation();
	private SmoothStepInterpolation() { }
	public float Evaluate( float normilizedDuration ) => MathAdapter.smoothStep( 0, 1, normilizedDuration );
}

public class SineInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction Instance = new SineInterpolation();
	private SineInterpolation() { }
	public float Evaluate( float normilizedDuration ) => ( MathAdapter.sin( ( normilizedDuration - .5f ) * MathAdapter.PI ) + 1 ) / 2;
}

public class PowerInterpolation : IInterpolationFunction
{
	//TODO: Optimize Iterpolation with fast operation
	public static readonly IInterpolationFunction Pow2 = Power2Interpolation.Instance;
	public static readonly IInterpolationFunction Sqrt = new PowerInterpolation( .5f );
	public static readonly IInterpolationFunction Pow3 = new PowerInterpolation( 3 );
	public static readonly IInterpolationFunction CubeRoot = new PowerInterpolation( .3333333f );
	private readonly float _power;
	public PowerInterpolation( float power ) { _power = power; }
	public float Evaluate( float normilizedDuration ) => Mathf.Pow( normilizedDuration, _power );

	private class Power2Interpolation : IInterpolationFunction
	{
		public static readonly IInterpolationFunction Instance = new Power2Interpolation();
		public Power2Interpolation() { }
		public float Evaluate( float normilizedDuration ) => normilizedDuration * normilizedDuration;
	}
}

public class SCurveInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction SCurveP2 = SCurveP2Interpolation.Instance;
	public static readonly IInterpolationFunction SCurveP3 = SCurveP3Interpolation.Instance;
	public static readonly IInterpolationFunction SCurveR2 = new SCurveInterpolation( .5f );
	public static readonly IInterpolationFunction SCurveR3 = new SCurveInterpolation( 1f/3f );
	private readonly float _power;
	public SCurveInterpolation( float power ) { _power = power; }
	public float Evaluate( float normilizedDuration ) 
	{
		var val = normilizedDuration * 2 - 1;
		var sign = Mathf.Sign( val );
		var aVal = sign * val;
		return ( Mathf.Pow( aVal, _power ) * sign + 1 ) / 2;
	}


	private class SCurveP2Interpolation : IInterpolationFunction
	{
		public static readonly IInterpolationFunction Instance = new SCurveP2Interpolation();
		public SCurveP2Interpolation() { }
		public float Evaluate( float normilizedDuration )
		{
			var val = normilizedDuration * 2 - 1;
			return ( val * val + 1 ) / 2;
		}
	}

	private class SCurveP3Interpolation : IInterpolationFunction
	{
		public static readonly IInterpolationFunction Instance = new SCurveP3Interpolation();
		public SCurveP3Interpolation() { }
		public float Evaluate( float normilizedDuration )
		{
			var val = normilizedDuration * 2 - 1;
			return ( val * val * val + 1 ) / 2;
		} 
	}
}

public class SinInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction Pi = new SinInterpolation( Mathf.PI );
	public static readonly IInterpolationFunction PiOver2 = new SinInterpolation( Mathf.PI / 2 );
	private readonly float _radAngle;
	private SinInterpolation( float radAngle ) { _radAngle = radAngle; }
	public float Evaluate( float normilizedDuration ) => Mathf.Sin( normilizedDuration * Mathf.PI / 2 );
}
