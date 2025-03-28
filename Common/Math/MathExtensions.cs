using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v2 = Unity.Mathematics.float2;
using v3 = Unity.Mathematics.float3;
using v4 = Unity.Mathematics.float4;
// using m33 = Unity.Mathematics.float3x3;
// using m44 = Unity.Mathematics.float4x4;
#else
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;
using v4 = UnityEngine.Vector4;
// using m33 = UnityEngine.Matrix3x3;
// using m44 = UnityEngine.Matrix4x4;
#endif

public static class MathExtensions
{
    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static v4 Float4At(this IList<float> array, int startId) => new v4(array[startId], array[startId + 1], array[startId + 2], array[startId + 3]);

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static v3 Float3At(this IList<float> array, int startId) => new v3(array[startId], array[startId + 1], array[startId + 2]);

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static v2 Float2At(this IList<float> array, int startId) => new v2(array[startId], array[startId + 1]);
}