using DG.Tweening;
using UnityEngine;

public class DoScale : ITweenAction<Transform>
{
    [SerializeField] Vector3 value = Vector3.one;

    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        return element.DOScale( value, duration ).SetEase( ease );
    }
}
