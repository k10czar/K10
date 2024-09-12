using DG.Tweening;
using UnityEngine;

public class DoLocalRotate : ITweenAction<Transform>
{
    [SerializeField] Vector3 value;

    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        return element.DOLocalRotate( value, duration ).SetEase( ease );
    }
}
