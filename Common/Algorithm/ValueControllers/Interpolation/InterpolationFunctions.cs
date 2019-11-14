using UnityEngine;

public interface IInterpolationFunction
{
	float Evaluate( float normilizedTime );
}

public enum ENormalizedValueInterpolation { Linear, Quadratic, SquareRoot, SCurve2, Cubic, CubeRoot, SCurve4 }

public static class InterpolationExtensions
{
	public static IInterpolationFunction GetFunction( this ENormalizedValueInterpolation type )
	{
		switch( type )
		{
			case ENormalizedValueInterpolation.Linear: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.Quadratic: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.SquareRoot: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.SCurve2: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.Cubic: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.CubeRoot: return LinearInterpolation.Instance;
			case ENormalizedValueInterpolation.SCurve4: return LinearInterpolation.Instance;
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

public class PowerInterpolation : IInterpolationFunction
{
	//TODO: Optimize Iterpolation with fast operation
	public static readonly IInterpolationFunction Pow2 = new PowerInterpolation( 2 );
	public static readonly IInterpolationFunction Sqrt = new PowerInterpolation( .5f );
	public static readonly IInterpolationFunction Pow3 = new PowerInterpolation( 3 );
	public static readonly IInterpolationFunction Cubic = new PowerInterpolation( .3333333f );
	private readonly float _power;
	public PowerInterpolation( float power ) { _power = power; }
	public float Evaluate( float normilizedDuration ) => Mathf.Pow( normilizedDuration, _power );
}

public class SCurveInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction SCurve2 = new SCurveInterpolation( 2 );
	public static readonly IInterpolationFunction SCurve4 = new SCurveInterpolation( 4 );
	private readonly float _power;
	public SCurveInterpolation( float power ) { _power = power; }
	public float Evaluate( float normilizedDuration ) => ( Mathf.Pow( normilizedDuration * 2 - 1, _power ) + 1 ) / 2;
}

public class SinInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction Pi = new SinInterpolation( Mathf.PI );
	public static readonly IInterpolationFunction PiOver2 = new SinInterpolation( Mathf.PI / 2 );
	private readonly float _radAngle;
	private SinInterpolation( float radAngle ) { _radAngle = radAngle; }
	public float Evaluate( float normilizedDuration ) => Mathf.Sin( normilizedDuration * Mathf.PI / 2 );
}
