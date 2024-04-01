using UnityEngine;

public interface IPositionGenerator
{
	(Vector3 pos, Quaternion rot) GetSpawn();
	Vector3 GetRandomPosition();
	Quaternion GetRandomRotation();
}