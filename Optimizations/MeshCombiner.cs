using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

#if TRY_USE_NEW_MATH
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
#endif

public enum ECombineType
{
    UnityMeshCombine = 0,
    CustomSingleThread = 1,
#if TRY_USE_NEW_MATH
    CustomJobsBurst = 2,
#endif
}

/// <summary>
/// Drop-in replacement for Mesh.CombineMeshes using AcquireReadOnlyMeshData +
/// AllocateWritableMeshData + ApplyAndDisposeWritableMeshData.
/// With TRY_USE_NEW_MATH: vertex transforms run multithreaded via Burst/Jobs.
/// Without TRY_USE_NEW_MATH: same pipeline, single-threaded on the main thread.
/// All instances are merged into a single submesh (same-material batching).
/// </summary>
public static class MeshCombiner
{
    public static Mesh Combine(IList<CombineInstance> instances, ECombineType combineType)
    {
        switch (combineType)
        {            
            case ECombineType.UnityMeshCombine:
                var mesh = new Mesh();
                mesh.CombineMeshes(instances.ToArray(), true, true);
                return mesh;
            case ECombineType.CustomSingleThread:
                return Combine(instances);
#if TRY_USE_NEW_MATH
            case ECombineType.CustomJobsBurst:
                return CombineJob(instances);
#endif
        }

        throw new System.ArgumentException("Invalid combine type");
    }


#if TRY_USE_NEW_MATH
    public static Mesh CombineJob(IList<CombineInstance> instances)
    {
        int n = instances.Count;
// #if CODE_METRICS
//             string CODE_TAG = $"MeshCombiner.Combine( {n} )";
//             CodeMetrics.Start( CODE_TAG );
// #endif
        var meshList = new List<Mesh>(n);
        for (int i = 0; i < n; i++)
            meshList.Add(instances[i].mesh);

        var srcData = Mesh.AcquireReadOnlyMeshData(meshList);

        var vertexOffsets  = new NativeArray<int>(n, Allocator.TempJob);
        var indexOffsets   = new NativeArray<int>(n, Allocator.TempJob);
        var subMeshIndices = new NativeArray<int>(n, Allocator.TempJob);
        var jobMatrices    = new NativeArray<float4x4>(n, Allocator.TempJob);

        int totalVerts   = 0;
        int totalIndices = 0;

        for (int i = 0; i < n; i++)
        {
            var inst   = instances[i];
            int subIdx = math.clamp(inst.subMeshIndex, 0, srcData[i].subMeshCount - 1);

            vertexOffsets[i]  = totalVerts;
            indexOffsets[i]   = totalIndices;
            subMeshIndices[i] = subIdx;
            jobMatrices[i]    = inst.transform;

            totalVerts   += srcData[i].vertexCount;
            totalIndices += (int)srcData[i].GetSubMesh(subIdx).indexCount;
        }

        var outputDataArray = Mesh.AllocateWritableMeshData(1);
        var outputData      = outputDataArray[0];

        outputData.SetVertexBufferParams(totalVerts,
            new VertexAttributeDescriptor(VertexAttribute.Position,  VertexAttributeFormat.Float32, 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal,    VertexAttributeFormat.Float32, 3, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.Tangent,   VertexAttributeFormat.Float32, 4, stream: 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, stream: 3));
        outputData.SetIndexBufferParams(totalIndices, IndexFormat.UInt32);

        var dstPositions = outputData.GetVertexData<float3>(0);
        var dstNormals   = outputData.GetVertexData<float3>(1);
        var dstTangents  = outputData.GetVertexData<float4>(2);
        var dstUVs       = outputData.GetVertexData<float2>(3);
        var dstIndices   = outputData.GetIndexData<uint>();

        var vertHandle = new CopyTransformVerticesJob
        {
            SrcMeshes     = srcData,
            Matrices      = jobMatrices,
            VertexOffsets = vertexOffsets,
            DstPositions  = dstPositions,
            DstNormals    = dstNormals,
            DstTangents   = dstTangents,
            DstUVs        = dstUVs,
        }.Schedule(n, 1);

        var idxHandle = new CopyIndicesJob
        {
            SrcMeshes      = srcData,
            VertexOffsets  = vertexOffsets,
            IndexOffsets   = indexOffsets,
            SubMeshIndices = subMeshIndices,
            DstIndices     = dstIndices,
        }.Schedule(n, 1);

        JobHandle.CombineDependencies(vertHandle, idxHandle).Complete();

        vertexOffsets.Dispose();
        indexOffsets.Dispose();
        subMeshIndices.Dispose();
        jobMatrices.Dispose();

        outputData.subMeshCount = 1;
        outputData.SetSubMesh(0, new SubMeshDescriptor(0, totalIndices));

        var combined = new Mesh();
        Mesh.ApplyAndDisposeWritableMeshData(outputDataArray, combined);
        combined.RecalculateBounds();

        srcData.Dispose();
// #if CODE_METRICS
//             CodeMetrics.Finish( CODE_TAG );
// #endif
        return combined;
    }

    // ─── Jobs ────────────────────────────────────────────────────────────────

    [BurstCompile]
    struct CopyTransformVerticesJob : IJobParallelFor
    {
        [ReadOnly] public Mesh.MeshDataArray    SrcMeshes;
        [ReadOnly] public NativeArray<float4x4> Matrices;
        [ReadOnly] public NativeArray<int>      VertexOffsets;

        [NativeDisableParallelForRestriction][NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> DstPositions;
        [NativeDisableParallelForRestriction][NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> DstNormals;
        [NativeDisableParallelForRestriction][NativeDisableContainerSafetyRestriction]
        public NativeArray<float4> DstTangents;
        [NativeDisableParallelForRestriction][NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> DstUVs;

        public void Execute(int index)
        {
            var src        = SrcMeshes[index];
            int vertCount  = src.vertexCount;
            int vertOffset = VertexOffsets[index];
            var matrix     = Matrices[index];
            var m3         = new float3x3(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz);
            var normalMat  = math.transpose(math.inverse(m3));

            var tempPos = new NativeArray<Vector3>(vertCount, Allocator.Temp);
            src.GetVertices(tempPos);
            var posF = tempPos.Reinterpret<float3>();
            for (int i = 0; i < vertCount; i++)
                DstPositions[vertOffset + i] = math.transform(matrix, posF[i]);
            tempPos.Dispose();

            if (src.HasVertexAttribute(VertexAttribute.Normal))
            {
                var tempNorm = new NativeArray<Vector3>(vertCount, Allocator.Temp);
                src.GetNormals(tempNorm);
                var normF = tempNorm.Reinterpret<float3>();
                for (int i = 0; i < vertCount; i++)
                    DstNormals[vertOffset + i] = math.normalizesafe(math.mul(normalMat, normF[i]));
                tempNorm.Dispose();
            }
            else
            {
                for (int i = 0; i < vertCount; i++)
                    DstNormals[vertOffset + i] = new float3(0, 1, 0);
            }

            if (src.HasVertexAttribute(VertexAttribute.Tangent))
            {
                var tempTan = new NativeArray<Vector4>(vertCount, Allocator.Temp);
                src.GetTangents(tempTan);
                var tanF = tempTan.Reinterpret<float4>();
                for (int i = 0; i < vertCount; i++)
                {
                    var t = tanF[i];
                    DstTangents[vertOffset + i] = new float4(math.normalizesafe(math.mul(m3, t.xyz)), t.w);
                }
                tempTan.Dispose();
            }
            else
            {
                for (int i = 0; i < vertCount; i++)
                    DstTangents[vertOffset + i] = new float4(1, 0, 0, 1);
            }

            if (src.HasVertexAttribute(VertexAttribute.TexCoord0))
            {
                var tempUV = new NativeArray<Vector2>(vertCount, Allocator.Temp);
                src.GetUVs(0, tempUV);
                var uvF = tempUV.Reinterpret<float2>();
                for (int i = 0; i < vertCount; i++)
                    DstUVs[vertOffset + i] = uvF[i];
                tempUV.Dispose();
            }
        }
    }

    [BurstCompile]
    struct CopyIndicesJob : IJobParallelFor
    {
        [ReadOnly] public Mesh.MeshDataArray SrcMeshes;
        [ReadOnly] public NativeArray<int>   VertexOffsets;
        [ReadOnly] public NativeArray<int>   IndexOffsets;
        [ReadOnly] public NativeArray<int>   SubMeshIndices;

        [NativeDisableParallelForRestriction][NativeDisableContainerSafetyRestriction]
        public NativeArray<uint> DstIndices;

        public void Execute(int index)
        {
            var src       = SrcMeshes[index];
            var sub       = src.GetSubMesh(SubMeshIndices[index]);
            int vertOff   = VertexOffsets[index];
            int writeHead = IndexOffsets[index];
            int baseVert  = sub.baseVertex + vertOff;
            int end       = sub.indexStart + sub.indexCount;

            if (src.indexFormat == IndexFormat.UInt16)
            {
                var srcIdx = src.GetIndexData<ushort>();
                for (int i = sub.indexStart; i < end; i++)
                    DstIndices[writeHead++] = (uint)(srcIdx[i] + baseVert);
            }
            else
            {
                var srcIdx = src.GetIndexData<uint>();
                for (int i = sub.indexStart; i < end; i++)
                    DstIndices[writeHead++] = srcIdx[i] + (uint)baseVert;
            }
        }
    }
#endif //TRY_USE_NEW_MATH

    public static Mesh Combine(IList<CombineInstance> instances)
    {
        int n = instances.Count;
// #if CODE_METRICS
//             string CODE_TAG = $"MeshCombiner.Combine( {n} )";
//             CodeMetrics.Start( CODE_TAG );
// #endif
        var meshList = new List<Mesh>(n);
        for (int i = 0; i < n; i++)
            meshList.Add(instances[i].mesh);

        var srcData = Mesh.AcquireReadOnlyMeshData(meshList);

        var vertexOffsets  = new int[n];
        var indexOffsets   = new int[n];
        var subMeshIndices = new int[n];

        int totalVerts   = 0;
        int totalIndices = 0;

        for (int i = 0; i < n; i++)
        {
            var inst   = instances[i];
            int subIdx = Mathf.Clamp(inst.subMeshIndex, 0, srcData[i].subMeshCount - 1);

            vertexOffsets[i]  = totalVerts;
            indexOffsets[i]   = totalIndices;
            subMeshIndices[i] = subIdx;

            totalVerts   += srcData[i].vertexCount;
            totalIndices += (int)srcData[i].GetSubMesh(subIdx).indexCount;
        }

        var outputDataArray = Mesh.AllocateWritableMeshData(1);
        var outputData      = outputDataArray[0];

        outputData.SetVertexBufferParams(totalVerts,
            new VertexAttributeDescriptor(VertexAttribute.Position,  VertexAttributeFormat.Float32, 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal,    VertexAttributeFormat.Float32, 3, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.Tangent,   VertexAttributeFormat.Float32, 4, stream: 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, stream: 3));
        outputData.SetIndexBufferParams(totalIndices, IndexFormat.UInt32);

        var dstPositions = outputData.GetVertexData<Vector3>(0);
        var dstNormals   = outputData.GetVertexData<Vector3>(1);
        var dstTangents  = outputData.GetVertexData<Vector4>(2);
        var dstUVs       = outputData.GetVertexData<Vector2>(3);
        var dstIndices   = outputData.GetIndexData<uint>();

        var tempPositions = new List<Vector3>();
        var tempNormals   = new List<Vector3>();
        var tempTangents  = new List<Vector4>();
        var tempUVs       = new List<Vector2>();

        for (int i = 0; i < n; i++)
        {
            var src       = srcData[i];
            int vertCount = src.vertexCount;
            int vertOff   = vertexOffsets[i];
            var matrix    = instances[i].transform;
            var normalMat = matrix.inverse.transpose;

            tempPositions.Clear();
            src.GetVertices(tempPositions);
            for (int v = 0; v < vertCount; v++)
                dstPositions[vertOff + v] = matrix.MultiplyPoint3x4(tempPositions[v]);

            if (src.HasVertexAttribute(VertexAttribute.Normal))
            {
                tempNormals.Clear();
                src.GetNormals(tempNormals);
                for (int v = 0; v < vertCount; v++)
                    dstNormals[vertOff + v] = normalMat.MultiplyVector(tempNormals[v]).normalized;
            }
            else
            {
                for (int v = 0; v < vertCount; v++)
                    dstNormals[vertOff + v] = Vector3.up;
            }

            if (src.HasVertexAttribute(VertexAttribute.Tangent))
            {
                tempTangents.Clear();
                src.GetTangents(tempTangents);
                for (int v = 0; v < vertCount; v++)
                {
                    var t  = tempTangents[v];
                    var wt = matrix.MultiplyVector(new Vector3(t.x, t.y, t.z)).normalized;
                    dstTangents[vertOff + v] = new Vector4(wt.x, wt.y, wt.z, t.w);
                }
            }
            else
            {
                for (int v = 0; v < vertCount; v++)
                    dstTangents[vertOff + v] = new Vector4(1, 0, 0, 1);
            }

            if (src.HasVertexAttribute(VertexAttribute.TexCoord0))
            {
                tempUVs.Clear();
                src.GetUVs(0, tempUVs);
                for (int v = 0; v < vertCount; v++)
                    dstUVs[vertOff + v] = tempUVs[v];
            }

            var sub       = src.GetSubMesh(subMeshIndices[i]);
            int baseVert  = sub.baseVertex + vertOff;
            int idxWrite  = indexOffsets[i];
            int end       = sub.indexStart + sub.indexCount;

            if (src.indexFormat == IndexFormat.UInt16)
            {
                var srcIdx = src.GetIndexData<ushort>();
                for (int idx = sub.indexStart; idx < end; idx++)
                    dstIndices[idxWrite++] = (uint)(srcIdx[idx] + baseVert);
            }
            else
            {
                var srcIdx = src.GetIndexData<uint>();
                for (int idx = sub.indexStart; idx < end; idx++)
                    dstIndices[idxWrite++] = srcIdx[idx] + (uint)baseVert;
            }
        }

        srcData.Dispose();

        outputData.subMeshCount = 1;
        outputData.SetSubMesh(0, new SubMeshDescriptor(0, totalIndices));

        var combined = new Mesh();
        Mesh.ApplyAndDisposeWritableMeshData(outputDataArray, combined);
        combined.RecalculateBounds();
// #if CODE_METRICS
//             CodeMetrics.Finish( CODE_TAG );
// #endif
        return combined;
    }
}
