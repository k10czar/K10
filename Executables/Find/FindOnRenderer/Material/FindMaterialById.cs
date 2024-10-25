using UnityEngine;
using System.Collections.Generic;

public class FindMaterialById : IFinder<Renderer,Material>
{
    [SerializeField] int materialId = 0;

    public IEnumerator<Material> Find(Renderer r)
    {
        if( materialId >= 0 && materialId < r.materials.Length )
            yield return r.materials[materialId];
    }
}
