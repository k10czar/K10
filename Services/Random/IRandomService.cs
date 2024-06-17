using UnityEngine;

public interface IRandomService : IService
{
	float NextFloat();
	Vector2 NextFloat2Direction();
}
