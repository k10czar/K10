using DG.Tweening;
using UnityEngine;

public class DoLocalMove : ITweenAction<Transform>
{
    [SerializeField] Vector3 value;

    public void Do(Transform element, in float duration, in Ease ease)
    {
        element.DOLocalMove( value, duration ).SetEase( ease );
    }
}