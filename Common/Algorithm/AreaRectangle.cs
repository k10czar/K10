using UnityEngine;
using System.Collections;

public class AreaRectangle : AreaManager
{
	[SerializeField] Vector2 _size = Vector2.one;
#if UNITY_EDITOR
	[SerializeField] Color _gizmoColor = Color.blue;
#endif

	public override float Area { get { var t = transform; var scl = t.lossyScale; return _size.y * scl.z * _size.x * scl.x; } }

	public override Vector3 RandomPosition()
	{
		var t = transform;
		var pos = t.position;
		var f = t.forward;
		var r = t.right;
		var scl = t.lossyScale;

		var pf = f * _size.y * scl.z * .5f;
		var pr = r * _size.x * scl.x * .5f;

		pos += ( K10Random.Value * 2 - 1 ) * pf + ( K10Random.Value * 2 - 1 ) * pr;
		return pos;
	}

	public override bool IsInsideProjection( Vector3 position )
	{
		return false;
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		var t = transform;
		var pos = t.position;
		var f = t.forward;
		var r = t.right;
		var scl = t.lossyScale;

		var pf = f * _size.y * scl.z * .5f;
		var pr = r * _size.x * scl.x * .5f;

		var p1 = pos + pf + pr;
		var p2 = pos + pf - pr;
		var p3 = pos - pf - pr;
		var p4 = pos - pf + pr;
		
		Debug.DrawLine( p1, p2, _gizmoColor );
		Debug.DrawLine( p2, p3, _gizmoColor );
		Debug.DrawLine( p3, p4, _gizmoColor );
		Debug.DrawLine( p4, p1, _gizmoColor );
	}
#endif
}
