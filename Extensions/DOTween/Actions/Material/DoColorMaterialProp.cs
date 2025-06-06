using DG.Tweening;
using UnityEngine;

public class DoColorMaterialProp : DoMaterialProp
{
    [SerializeField] Color color;

    public override Tweener Do(Material element, in float duration, in Ease ease)
    {
        Debug.Log( $"{element.NameOrNull()}.DOColor( {PropertyID}, {color} )" );
        return element.DOColor( color, PropertyID, duration ).SetEase( ease );
    }
}
