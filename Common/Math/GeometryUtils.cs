using System.Runtime.CompilerServices;
using UnityEngine;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v2 = Unity.Mathematics.float2;
using v3 = Unity.Mathematics.float3;
#else
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;
#endif

public static class GeometryUtils
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;

	[MethodImpl(AggrInline)] public static float DistanceToLine(Ray ray, v3 point, out v3 projectedPoint)
	{
		var t = point - ((v3)ray.origin);
		var dl = MathAdapter.length(MathAdapter.cross(ray.direction, t));
		var dot = MathAdapter.dot(ray.direction, t);
		projectedPoint = ray.origin + ray.direction * dot;
		return dl;
	}

	[MethodImpl(AggrInline)] public static float DistanceToLine(Ray ray, v3 point)
	{
		return MathAdapter.length(MathAdapter.cross(ray.direction, point - ((v3)ray.origin)));
	}

	[MethodImpl(AggrInline)] public static v3 PlaneClosestPoint(v3 position, v3 planeNormal, v3 planeOrigin)
	{
		var originToPoint = position - planeOrigin;
		var dot = MathAdapter.dot( planeNormal, originToPoint );
		return position - dot * planeNormal;
	}

	[MethodImpl(AggrInline)] public static bool Aprox(float a, float b) => MathAdapter.Approximately(a, b);

	[MethodImpl(AggrInline)] public static bool NormalsAreParallel(v3 planeNormal, v3 originToPoint)
	{
		return (Aprox(planeNormal.x, originToPoint.x) && Aprox(planeNormal.y, originToPoint.y) && Aprox(planeNormal.z, originToPoint.z)) ||
				(Aprox(planeNormal.x, -originToPoint.x) && Aprox(planeNormal.y, -originToPoint.y) && Aprox(planeNormal.z, -originToPoint.z));
	}


	[MethodImpl(AggrInline)] public static bool PlaneLineIntersectPoint(v3 randomPlanePoint, v3 planeNormal, v3 lineOrigin, v3 lineDir, ref v3 intersectionPoint, out bool anyLinePoint)
	{
		var proj = MathAdapter.dot(planeNormal, lineDir);
		var originToPoint = lineOrigin - randomPlanePoint;
		if (Aprox(proj, 0)) // Check if the Plane normal and line direction are perpendicular
		{
			// var originToPointDir = MathAdapter.normalize(originToPoint);
			var pointProj = MathAdapter.dot(planeNormal, originToPoint);
			if (Aprox(pointProj, 0)) //Check if line origin is in the plane, if it is any point in line intersect with the plane
			{
				anyLinePoint = true;
				intersectionPoint = lineOrigin;
				return true;
			}
			anyLinePoint = false;
			return false;
		}
		//Find the only intersection point

		var fac = -MathAdapter.dot(planeNormal, originToPoint) / proj;
		var u = lineDir * fac;
		intersectionPoint = lineOrigin + u;

		anyLinePoint = false;
		return true;
	}
}
