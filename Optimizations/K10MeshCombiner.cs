#if CODE_METRICS
#define DEBUG_VERBOSE
#endif

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
public static class K10MeshCombiner
{
    public static int CountVertices(this IList<CombineInstance> instances)
    {
        int count = 0;
        for (int i = 0; i < instances.Count; i++)
            count += instances[i].mesh.vertexCount;
        return count;
    }

    public static Mesh Combine(IList<CombineInstance> instances, ECombineType combineType)
    {
        switch (combineType)
        {            
            case ECombineType.UnityMeshCombine:
                var mesh = new Mesh();
                if( instances.CountVertices() > 65535 )
                    mesh.indexFormat = IndexFormat.UInt32;
                mesh.CombineMeshes(instances.ToArray(), true, true);
                return mesh;
            case ECombineType.CustomSingleThread:
                return CombineCustonSingleThread(instances);
#if TRY_USE_NEW_MATH
            case ECombineType.CustomJobsBurst:
                return CombineJobBurst(instances);
#endif
        }

        throw new System.ArgumentException("Invalid combine type");
    }

    public static void MergeGroup(
        IList<MeshFilter> filters,
        Material material,
        Transform parent,
        int predictedVerts,
        ECombineType combineType = ECombineType.UnityMeshCombine,
        List<GameObject> objectsExecuted = null,
        ShadowCastingMode? shadowCasting = null,
        bool? receiveShadows = null )
    {
#if UNITY_EDITOR
        bool hasStaticBatching = UnityEditor.PlayerSettings.GetStaticBatchingForPlatform( UnityEditor.EditorUserBuildSettings.activeBuildTarget );
#endif
        var count = filters.Count;
        var combine = new List<CombineInstance>();
        bool castShadows = false;
        int verts = 0;
        int layer = 0;
        var matWorldToLocal = parent.worldToLocalMatrix;
        var SB = StringBuilderPool.RequestWith( $"MergeGroup: {predictedVerts}v {material}\n" );

        for (int j = 0; j < count; j++)
        {
            var filter = filters[j];
            if (filter == null) continue;

#if UNITY_EDITOR
            if (hasStaticBatching && filter.gameObject.isStatic)
                Debug.LogError( $"filter {filter.HierarchyNameOrNull()} is static but staticBatching is enabled — remove from merge group or disable Static flag", filter );
#endif

            layer = filter.gameObject.layer;
            var meshRenderer = filter.GetComponent<MeshRenderer>();
            var instMesh = filter.sharedMesh;

            if (instMesh == null)
            {
                SB.AppendLine( $"{filter.HierarchyNameOrNull()} with null sharedMesh" );
                Debug.LogError( $"filter {filter.HierarchyNameOrNull()} does not has sharedMesh", filter );
                continue;
            }

            if (!instMesh.isReadable)
            {
                SB.AppendLine( $"{filter.HierarchyNameOrNull()} with not readable sharedMesh" );
                Debug.LogError( $"filter {filter.HierarchyNameOrNull()} has not readable sharedMesh", filter );
                continue;
            }

            combine.Add( new CombineInstance
            {
                mesh      = instMesh,
                transform = matWorldToLocal * filter.transform.localToWorldMatrix,
            } );

            verts += instMesh.vertexCount;

            if (meshRenderer.shadowCastingMode.Equals( ShadowCastingMode.On ))
                castShadows = true;

            meshRenderer.enabled = false;
            objectsExecuted?.Add( filter.gameObject );

            SB.AppendLine( $"{instMesh.name}({instMesh.vertexCount}) from {filter.HierarchyNameOrNull()}" );
        }

        if (combine.Count == 0) return;

        var combinedMesh = Combine( combine, ECombineType.UnityMeshCombine );
        combinedMesh.name = $"[Merge] {combine.Count}/{count} {parent.gameObject.name} {material.name}_mesh";

        var combineGameObject = new GameObject( $"[Merge] {combine.Count}/{count} {parent.gameObject.name} {material.name} v:{verts}" );
        var newT = combineGameObject.transform;
        newT.SetParent( parent );
        newT.localPosition    = Vector3.zero;
        newT.localScale       = Vector3.one;
        newT.localEulerAngles = Vector3.zero;

        combineGameObject.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
        var combinedMeshRenderer = combineGameObject.AddComponent<MeshRenderer>();
        combinedMeshRenderer.sharedMaterial    = material;
        // Use the caller-supplied mode when the group is homogeneous by shadow casting; otherwise fall
        // back to the auto rule (cast if any source mesh casts).
        combinedMeshRenderer.shadowCastingMode = shadowCasting ?? (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
        if (receiveShadows.HasValue)
            combinedMeshRenderer.receiveShadows = receiveShadows.Value;

        combineGameObject.layer    = layer;
        combineGameObject.isStatic = true;

#if DEBUG_VERBOSE
        var diff = verts - predictedVerts;
        if (diff != 0) Debug.Log( $"{combineGameObject.name}:: <color=magenta>diff({diff})</color> {SB.ReturnToPoolAndCast()}", combineGameObject );
        else           Debug.Log( $"{combineGameObject.name}:: diff({diff}) {SB.ReturnToPoolAndCast()}", combineGameObject );
#endif

        if (Application.isPlaying)
        {
            for (int j = 0; j < count; j++)
            {
                var filter = filters[j];
                if (filter == null) continue;
                var meshRenderer = filter.GetComponent<MeshRenderer>();
                if (meshRenderer.enabled) continue;
                Object.Destroy( filter );
                if (meshRenderer != null)
                    Object.Destroy( meshRenderer );
            }
        }
    }

#if TRY_USE_NEW_MATH
    public static Mesh CombineJobBurst(IList<CombineInstance> instances)
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

    public static Mesh CombineCustonSingleThread(IList<CombineInstance> instances)
    {
        int n = instances.Count;

        int totalVerts = 0;
        var subMeshIndices = new int[n];
        for (int i = 0; i < n; i++)
        {
            var mesh = instances[i].mesh;
            subMeshIndices[i] = Mathf.Clamp(instances[i].subMeshIndex, 0, mesh.subMeshCount - 1);
            totalVerts += mesh.vertexCount;
        }

        var positions = new List<Vector3>(totalVerts);
        var normals   = new List<Vector3>(totalVerts);
        var tangents  = new List<Vector4>(totalVerts);
        var uvs       = new List<Vector2>(totalVerts);
        var indices   = new List<int>();

        int vertOffset = 0;

        for (int i = 0; i < n; i++)
        {
            var inst      = instances[i];
            var mesh      = inst.mesh;
            var matrix    = inst.transform;
            var normalMat = matrix.inverse.transpose;
            int subIdx    = subMeshIndices[i];
            int vc        = mesh.vertexCount;

            var srcVerts = mesh.vertices;
            for (int v = 0; v < vc; v++)
                positions.Add(matrix.MultiplyPoint3x4(srcVerts[v]));

            var srcNorms = mesh.normals;
            if (srcNorms.Length == vc)
            {
                for (int v = 0; v < vc; v++)
                    normals.Add(normalMat.MultiplyVector(srcNorms[v]).normalized);
            }
            else
            {
                for (int v = 0; v < vc; v++)
                    normals.Add(Vector3.up);
            }

            var srcTans = mesh.tangents;
            if (srcTans.Length == vc)
            {
                for (int v = 0; v < vc; v++)
                {
                    var t  = srcTans[v];
                    var wt = matrix.MultiplyVector(new Vector3(t.x, t.y, t.z)).normalized;
                    tangents.Add(new Vector4(wt.x, wt.y, wt.z, t.w));
                }
            }
            else
            {
                for (int v = 0; v < vc; v++)
                    tangents.Add(new Vector4(1, 0, 0, 1));
            }

            var srcUVs = mesh.uv;
            if (srcUVs.Length == vc)
            {
                for (int v = 0; v < vc; v++)
                    uvs.Add(srcUVs[v]);
            }
            else
            {
                for (int v = 0; v < vc; v++)
                    uvs.Add(Vector2.zero);
            }

            var srcTris = mesh.GetTriangles(subIdx);
            for (int t = 0; t < srcTris.Length; t++)
                indices.Add(srcTris[t] + vertOffset);

            vertOffset += vc;
        }

        var combined = new Mesh();
        if (totalVerts > 65535) combined.indexFormat = IndexFormat.UInt32;
        combined.SetVertices(positions);
        combined.SetNormals(normals);
        combined.SetTangents(tangents);
        combined.SetUVs(0, uvs);
        combined.SetTriangles(indices, 0);
        combined.RecalculateBounds();
        return combined;
    }
}
