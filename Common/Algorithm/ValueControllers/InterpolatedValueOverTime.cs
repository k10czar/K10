using UnityEngine;

public interface IInterpolatedValueOverTime
{
	float Value { get; }
	float Seconds { get; }
	IInterpolationFunction Interpolation { get; }
}

[System.Serializable]
public class InterpolatedValueOverTime : IInterpolatedValueOverTime
{
	[SerializeField] private float _value = 0;
	[SerializeField] private float _seconds = 0;
	[SerializeField] ENormalizedValueInterpolation _inpterpolation;

	public InterpolatedValueOverTime( float value, float seconds = 0, ENormalizedValueInterpolation iterpolation = ENormalizedValueInterpolation.Linear )
	{
		_value = value;
		_seconds = seconds;
		_inpterpolation = iterpolation;
	}

	public float Value => _value;
	public float Seconds => _seconds;
	public IInterpolationFunction Interpolation => _inpterpolation.GetFunction();
}
