using DG.Tweening;
using UnityEngine;

public class DoLocalMove : ITweenAction<Transform>
{
    [SerializeField] Vector3 value;

    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        return element.DOLocalMove( value, duration ).SetEase( ease );
    }
}