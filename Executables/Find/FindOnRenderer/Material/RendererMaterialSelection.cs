using UnityEngine;
using System.Collections.Generic;

public abstract class RendererMaterialSelection : SelectWithPredicate<Renderer,Material>
{
    public override IEnumerable<Material> GetEnumeration( Renderer r ) => r.materials;
}
