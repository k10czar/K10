using DG.Tweening;
using UnityEngine;

public class DoLocalRotate : ITweenAction<Transform>
{
    [SerializeField] Vector3 value;

    public void Do(Transform element, in float duration, in Ease ease)
    {
        element.DOLocalRotate( value, duration ).SetEase( ease );
    }
}
