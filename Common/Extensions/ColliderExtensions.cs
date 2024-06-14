using System;
using UnityEngine;

public static class ColliderExtensions
{
    public static Vector3 SafeClosestPoint(this CapsuleCollider target, Vector3 point)
    {
        target.GetCapsulePoints(out Vector3 bot, out Vector3 top, out float radius);
        var corePoint = point.OnLineSegment(bot, top);
        var coreDist = (point - corePoint);
        var coreMag = coreDist.magnitude;
        if (coreMag > radius) coreDist = coreDist * (radius / coreMag);
        return corePoint + coreDist;
    }

    public static void GetCapsulePoints(this CapsuleCollider target, out Vector3 bot, out Vector3 top, out float radius)
    {
        if (target.direction != 1) throw new NotImplementedException("Can't handle capsule non-Y direction!");

        radius = target.radius;
        GetCapsulePoints(target.transform, target.center, target.height, out bot, out top, ref radius);
    }

    public static void GetCapsulePoints(this CharacterController target, out Vector3 bot, out Vector3 top, out float radius)
    {
        radius = target.radius;
        GetCapsulePoints(target.transform, target.center, target.height, out bot, out top, ref radius);
    }

    private static void GetCapsulePoints(Transform transform, Vector3 centerOffset, float height, out Vector3 bot, out Vector3 top, ref float radius)
    {
        var halfHeight = Mathf.Max(height * .5f - radius, 0);

        top = transform.TransformPoint(centerOffset + Vector3.up * halfHeight);
        bot = transform.TransformPoint(centerOffset + Vector3.down * halfHeight);

        var scale = transform.lossyScale;
        radius *= Mathf.Max(scale.x, scale.z);
    }
}