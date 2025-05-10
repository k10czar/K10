using UnityEngine;

public class SelectMaterialWithSameNameOf : RendererMaterialSelection
{
    [SerializeField] Material material;
    protected override bool Predicate(Material m) => material != null && m.name == material.name;
}
