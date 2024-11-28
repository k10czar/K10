using UnityEngine;

public class SpawnAreaFromTransform : IPositionGenerator, IDrawGizmosOnSelected, IDrawGizmos
{
	[SerializeField] Transform _transform;
	[SerializeField] float _radius = 1;
	[SerializeField] bool _debugGizmos;

    public (Vector3 pos, Quaternion rot) GetSpawn() => ( GetRandomPosition(), GetRandomRotation() );

    public Vector3 GetRandomPosition()
    {
		var rndServ = RandomService.Current;
		var rndR = _radius * rndServ.NextFloat();
		var rndDir = rndServ.NextFloat2Direction();
		var origin = _transform.position;
		return origin + _transform.forward * ( rndDir.y * rndR) + _transform.right * (rndDir.x * rndR);
    }

    public Quaternion GetRandomRotation()
    {
        return _transform.rotation;
    }

#if UNITY_EDITOR
	public void OnDrawGizmosSelected()
	{
		DrawGizmos( Color.yellow );
	}

	public void OnDrawGizmos()
	{
		if(_debugGizmos) DrawGizmos(Color.blue);
	}

	public void DrawGizmos(Color color)
	{
		if (_transform == null) return;
		DebugUtils.Circle(_transform.position, _transform.forward, _transform.up, _radius, color, false);
	}
#endif
}