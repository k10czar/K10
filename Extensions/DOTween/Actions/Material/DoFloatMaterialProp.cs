using DG.Tweening;
using UnityEngine;

public class DoFloatMaterialProp : DoMaterialProp
{
    [SerializeField] float endValue;

    public override Tweener Do(Material element, in float duration, in Ease ease)
    {
        return element.DOFloat( endValue, PropertyID, duration ).SetEase( ease );
    }
}
