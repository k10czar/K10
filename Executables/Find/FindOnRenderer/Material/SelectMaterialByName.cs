using UnityEngine;

public class SelectMaterialByName : RendererMaterialSelection
{
    [SerializeField] string name = "Standard";
    protected override bool Predicate(Material m) => m.name == name;
}