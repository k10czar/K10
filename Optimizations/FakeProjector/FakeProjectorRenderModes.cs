using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// How a decal combines with the surface underneath it. Mirrors the legacy projector shader family, which is
/// what the projectors being replaced were using.
/// </summary>
public enum EFakeProjectorRenderMode
{
    /// <summary>Standard alpha blend — the decal covers the surface where it is opaque.</summary>
    Diffuse = 0,

    /// <summary>Adds light. Blob lights, glows, magic circles.</summary>
    Additive = 1,

    /// <summary>Darkens. Blob shadows, grime, scorch marks.</summary>
    Multiply = 2,
}

/// <summary>
/// Supplies the shared material for each render mode, so a <see cref="FakeProjector"/> works with nothing
/// assigned and every decal of the same mode and texture collapses into one instanced draw.
/// </summary>
/// <remarks>
/// <para>
/// One material per mode, shared by every projector using it. That is the point: the batcher keys batches on
/// the material/texture pair, so per-projector materials would fragment batching into nothing, while a handful
/// of shared ones let a whole level's decals go out in a few draws.
/// </para>
/// <para>
/// Assigning a material on the component overrides this — for anything these modes don't cover. Such a
/// material must have GPU instancing enabled or the batcher will skip it.
/// </para>
/// </remarks>
public static class FakeProjectorRenderModes
{
    /// <summary>Loaded from Resources so the shader survives build stripping — nothing in a scene references it.</summary>
    const string SHADER_RESOURCE_PATH = "FakeProjectorDecal";

    static readonly Dictionary<EFakeProjectorRenderMode, Material> _materials = new();
    static Shader _shader;
    static bool _shaderMissingReported;

    /// <summary>
    /// The shared material for a render mode, created on first use. Null only when the shader failed to load,
    /// which the batcher treats as nothing to draw.
    /// </summary>
    public static Material Get( EFakeProjectorRenderMode mode )
    {
        if( _materials.TryGetValue( mode, out var material ) && material != null ) return material;

        var shader = GetShader();
        if( shader == null ) return null;

        material = new Material( shader )
        {
            name = $"FakeProjector {mode}",
            hideFlags = HideFlags.HideAndDontSave,
            enableInstancing = true,
        };

        ApplyBlend( material, mode );
        _materials[mode] = material;
        return material;
    }

    static void ApplyBlend( Material material, EFakeProjectorRenderMode mode )
    {
        switch( mode )
        {
            case EFakeProjectorRenderMode.Additive:
                SetBlend( material, BlendMode.SrcAlpha, BlendMode.One, multiply: false );
                break;

            case EFakeProjectorRenderMode.Multiply:
                SetBlend( material, BlendMode.DstColor, BlendMode.Zero, multiply: true );
                break;

            default:
                SetBlend( material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, multiply: false );
                break;
        }
    }

    static void SetBlend( Material material, BlendMode source, BlendMode destination, bool multiply )
    {
        material.SetFloat( "_SrcBlend", (float)source );
        material.SetFloat( "_DstBlend", (float)destination );
        material.SetFloat( "_MultiplyMode", multiply ? 1f : 0f );
    }

    static Shader GetShader()
    {
        if( _shader != null ) return _shader;

        _shader = Resources.Load<Shader>( SHADER_RESOURCE_PATH );
        if( _shader == null && !_shaderMissingReported )
        {
            _shaderMissingReported = true;
            Debug.LogError( $"Could not load '{SHADER_RESOURCE_PATH}' from Resources — fake projectors using a default render mode cannot render." );
        }

        return _shader;
    }
}
