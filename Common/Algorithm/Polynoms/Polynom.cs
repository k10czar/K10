using UnityEngine;

[System.Serializable]
public class Polynom
{
	[SerializeField] float[] _parameters;

	public virtual float Evaluate( float x )
	{
		var value = 0f;
		var xPow = 1f;
		for( int i = 0; i < _parameters.Length; i++ )
		{
			value += _parameters[i] * xPow;
			xPow *= x;
		}
		return value;
	}
}
