using UnityEngine;

[System.Serializable]
public class ClampedPolynom : Polynom
{
	[SerializeField] float _min = float.MinValue;
	[SerializeField] float _max = float.MaxValue;

	public override float Evaluate( float x ) => Mathf.Clamp( base.Evaluate( x ), _min, _max );
}