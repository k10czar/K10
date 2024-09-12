using DG.Tweening;
using UnityEngine;

public class DoPunchRotation : ITweenAction<Transform>
{
    [SerializeField] Vector3 punch = Vector3.one;
    [SerializeField] int vibrato = 10;
    [SerializeField] float elasticity = 1;

    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        return element.DOPunchRotation( punch, duration, vibrato, elasticity ).SetEase( ease );
    }
}