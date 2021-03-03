using UnityEngine;

[System.Serializable]
public class WeightedPolynom : Polynom
{
	[SerializeField] float _weight = 1;

	public override float Evaluate( float x ) => base.Evaluate( x ) * _weight;
}
