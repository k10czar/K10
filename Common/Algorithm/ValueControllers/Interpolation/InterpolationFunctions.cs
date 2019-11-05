using UnityEngine;

public interface IInterpolationFunction
{
	float Evaluate( float normilizedTime );
}

public class LinearInterpolation : IInterpolationFunction
{
	public static readonly IInterpolationFunction Instance = new LinearInterpolation();
	private LinearInterpolation() { }
	public float Evaluate( float normilizedDuration ) => normilizedDuration;
}

public class PowerInterpolation : IInterpolationFunction
{
	private readonly float _power;
	public PowerInterpolation( float power ) { _power = power; }
	public float Evaluate( float normilizedDuration ) => Mathf.Pow( normilizedDuration, _power );
}

public class SCurveInterpolation : IInterpolationFunction
{
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
