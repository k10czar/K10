using UnityEngine;

public static class PhysicsCache
{
	public const int MAX_COLISION_CHECK = 16;
	public static readonly RaycastHit[] rayHits = new RaycastHit[MAX_COLISION_CHECK];
	public static readonly Collider[] colliders = new Collider[MAX_COLISION_CHECK];
}