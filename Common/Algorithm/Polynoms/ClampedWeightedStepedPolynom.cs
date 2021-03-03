using UnityEngine;

[System.Serializable]
public class ClampedWeightedStepedPolynom : ClampedWeightedPolynom
{
	[SerializeField] float _xStep = 1;

	public override float Evaluate( float x ) => base.Evaluate( (int)( x / _xStep ) );
}