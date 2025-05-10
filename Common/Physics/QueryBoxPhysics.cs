using UnityEngine;


[System.Serializable]
public class QueryBoxPhysics
{
	[SerializeField] private Transform transform;
	[SerializeField] private Vector3 dimensions = Vector3.one;
	[SerializeField] private Vector3 offset = Vector3.zero;

	public Transform Transform => transform;

	public Vector3 Center => transform.position + transform.rotation * offset;
	public Vector3 Dimensions => dimensions;
	public Quaternion Rotation => transform.rotation;

	public void DrawGizmos( Color color )
	{
		if( transform == null ) return;
		var rot = transform.rotation;
		var center = transform.position + rot * offset;
		DebugUtils.Gizmos.WireBox( center, rot, dimensions, color );
		GizmosColorManager.New( Color.gray );
		Gizmos.DrawWireSphere( center, dimensions.magnitude / 2 );
		Gizmos.DrawWireSphere( center, Mathf.Min( dimensions.x, dimensions.y, dimensions.z ) / 2 );
		GizmosColorManager.Revert();
	}

	public Vector3 ClosestPoint( Ray ray )
	{
		return ray.ClosestPoint( Center );
	}

	public bool Contains( Vector3 point )
	{
		var rot = transform.rotation;
		var center = transform.position + rot * offset;

		var maxSqrtDist = dimensions.sqrMagnitude * .25f;
		
		var delta = point - center;
		var deltaSqrMag = delta.sqrMagnitude;
		if( deltaSqrMag > maxSqrtDist ) return false;

		var absDX = Mathf.Abs( dimensions.x );
		var minDim = Mathf.Abs( dimensions.x );
		var absDY = Mathf.Abs( dimensions.y );
		if( minDim > absDY ) minDim = absDY;
		var absDZ = Mathf.Abs( dimensions.z );
		if( minDim > absDZ ) minDim = absDZ;
		
		if( deltaSqrMag < minDim * minDim * .25f ) return true;

		var dx = Vector3.Dot( rot * Vector3.right, delta );
		if( dx > absDX * .5f || dx < -absDX * .5f ) return false;
		var dy = Vector3.Dot( rot * Vector3.up, delta );
		if( dy > absDY * .5f || dy < -absDY * .5f ) return false;
		var dz = Vector3.Dot( rot * Vector3.forward, delta );
		if( dz > absDZ * .5f || dz < -absDZ * .5f ) return false;

		return true;
	}

	public bool IsIntersecting( Ray ray )
	{
		var rot = transform.rotation;
		var center = transform.position + rot * offset;
		var maxSqrtDist = dimensions.sqrMagnitude * .25f;
		var closestPoint = ray.ClosestPoint( center );
		var delta = closestPoint - center;
		var deltaSqrMag = delta.sqrMagnitude;
		if( deltaSqrMag > maxSqrtDist ) return false;

		var absDX = Mathf.Abs( dimensions.x );
		var minDim = Mathf.Abs( dimensions.x );
		var absDY = Mathf.Abs( dimensions.y );
		if( minDim > absDY ) minDim = absDY;
		var absDZ = Mathf.Abs( dimensions.z );
		if( minDim > absDZ ) minDim = absDZ;

		if( deltaSqrMag < minDim * minDim * .25f ) return true;

		var br = rot * Vector3.right;
		var bu = rot * Vector3.up;
		var bf = rot * Vector3.forward;
		
		var roBS = ray.origin - center;
		var oDir = ray.direction;

		//Transform the ray to an box space where the center of box is on (0,0,0) and the dimension is (1,1,1)
		var originOnBoxSpace = new Vector3( Vector3.Dot( br, roBS ) / dimensions.x,
											Vector3.Dot( bu, roBS ) / dimensions.y,
											Vector3.Dot( bf, roBS ) / dimensions.z );

		var directionOnBoxSpace = new Vector3( Vector3.Dot( br, oDir ) / dimensions.x,
												Vector3.Dot( bu, oDir ) / dimensions.y,
												Vector3.Dot( bf, oDir ) / dimensions.z ).normalized;

		var boxSpaceRay = new Ray( originOnBoxSpace, directionOnBoxSpace );

		var boxSpacePos = boxSpaceRay.ClosestPoint( Vector3.zero );

		var rescaled = Vector3.Scale( boxSpacePos, dimensions );
		var wpos = br * rescaled.x + bu * rescaled.y + bf * rescaled.z;
		Gizmos.DrawWireSphere( center + wpos, .005f );

		if( boxSpacePos.x > .5f || boxSpacePos.x < -.5f ) return false;
		if( boxSpacePos.y > .5f || boxSpacePos.y < -.5f ) return false;
		if( boxSpacePos.z > .5f || boxSpacePos.z < -.5f ) return false;

		Gizmos.DrawRay( center, wpos );
		Gizmos.DrawRay( center, br * boxSpacePos.x * dimensions.x );
		Gizmos.DrawRay( center, bu * boxSpacePos.y * dimensions.y );
		Gizmos.DrawRay( center, bf * boxSpacePos.z * dimensions.z );

		return true;
	}
}
