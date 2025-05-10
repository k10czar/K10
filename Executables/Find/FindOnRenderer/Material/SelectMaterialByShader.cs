using UnityEngine;

public class SelectMaterialByShader : RendererMaterialSelection
{
    [SerializeField] Shader shader;
    protected override bool Predicate(Material m) => m.shader == shader;
}
