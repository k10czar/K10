using DG.Tweening;
using UnityEditor;
using UnityEngine;

public abstract class DoMaterialProp : ITweenAction<Material>
{
    [SerializeReference,ExtendedDrawer] IShaderProperty propetyFindStrategy;

    protected int PropertyID => propetyFindStrategy?.PropertyID ?? -1;

    public abstract Tweener Do(Material element, in float duration, in Ease ease);
}