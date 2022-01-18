using System.Runtime.CompilerServices;
using UnityEngine;

public static class PhysicsExtension
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;

	[MethodImpl(AggrInline)]
	public static Vector3 AxisUp( this CapsuleCollider capsule )
	{
		switch( capsule.direction )
		{
			case 0: return capsule.transform.right;
			case 1: return capsule.transform.up;
			case 2: return capsule.transform.forward;
		}
		return capsule.transform.up;
	}
}
