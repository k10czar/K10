using DG.Tweening;
using UnityEngine;

public class DoFadeMaterialProp : DoMaterialProp
{
    [SerializeField,Range(0,1)] float endValue;

    public override Tweener Do(Material element, in float duration, in Ease ease)
    {
        return element.DOFade( endValue, PropertyID, duration ).SetEase( ease );
    }
}
