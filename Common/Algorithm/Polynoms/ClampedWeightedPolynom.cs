using UnityEngine;

[System.Serializable]
public class ClampedWeightedPolynom : ClampedPolynom
{
	[SerializeField] float _weight = 1;

	public override float Evaluate( float x ) => base.Evaluate( x ) * _weight;
}
