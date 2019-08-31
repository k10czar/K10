using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IAreaManager
{
	float Area { get; }
	Vector3 RandomPosition();
	bool IsInsideProjection( Vector3 position );
}

public abstract class AreaManager : MonoBehaviour, IAreaManager
{
	public abstract float Area { get; }
	public abstract Vector3 RandomPosition();
	public abstract bool IsInsideProjection( Vector3 position );
}

[System.Serializable]
public class AreaCollection : IAreaManager
{
	[SerializeField] List<AreaManager> _areas = new List<AreaManager>();

	public float Area { get { float w = 0; for( int i = 0; i < _areas.Count; i++ ) w += _areas[i].Area; return w; } }

	public Vector3 RandomPosition()
	{
		var totalWeight = Area;

		float rnd = K10Random.Value * totalWeight;
		for( int i = 0; i < _areas.Count; i++ )
		{
			var s = _areas[i];
			rnd -= s.Area;
			if( rnd <= 0 )
				return s.RandomPosition();
		}

		return Vector3.zero;
    }

	public bool IsInsideProjection( Vector3 position )
	{
		for( int i = 0; i < _areas.Count; i++ )
			if( _areas[i].IsInsideProjection( position ) )
				return true;

		return false;
    }
}

public class AreaCircumference : AreaManager
{
	[SerializeField] float _min = 100;
	[SerializeField] float _max = 200;

#if UNITY_EDITOR
	[SerializeField] Color _gizmoColor = Color.blue;
#endif

	public override float Area { get { return Mathf.PI *_max * _max - Mathf.PI * _min * _min; } }

	public override Vector3 RandomPosition()
	{
		var rndAng = K10Random.Value * 2 * Mathf.PI;
		var rndRadius = _min + K10Random.Value * ( _max - _min );
		var t = transform;
		var pos = t.position;
		var f = t.forward;
		var r = t.right;

		pos += f * rndRadius * Mathf.Cos( rndAng ) + r * rndRadius * Mathf.Sin( rndAng );
//#if UNITY_EDITOR
		//DebugUtils.X( pos, f, t.up, 1, _gizmoColor );
//#endif

		return t.position + t.forward * rndRadius * Mathf.Cos( rndAng ) + t.right * rndRadius * Mathf.Sin( rndAng );
	}

	public override bool IsInsideProjection( Vector3 position )
	{
		return false;
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		var rot = transform.rotation;
		DebugUtils.Circle( transform.position, rot * Vector3.forward, rot * Vector3.up, _min, _gizmoColor, false, 5 * Mathf.Deg2Rad );
		DebugUtils.Circle( transform.position, rot * Vector3.forward, rot * Vector3.up, _max, _gizmoColor, false, 5 * Mathf.Deg2Rad );
	}
#endif
}
