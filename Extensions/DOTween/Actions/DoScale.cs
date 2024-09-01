using DG.Tweening;
using UnityEngine;

public class DoScale : ITweenAction<Transform>
{
    [SerializeField] Vector3 value = Vector3.one;

    public void Do(Transform element, in float duration, in Ease ease)
    {
        element.DOScale( value, duration ).SetEase( ease );
    }
}
