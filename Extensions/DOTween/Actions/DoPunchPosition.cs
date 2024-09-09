using DG.Tweening;
using UnityEngine;

public class DoPunchPosition : ITweenAction<Transform>
{
    [SerializeField] Vector3 punch = Vector3.one;
    [SerializeField] int vibrato = 10;
    [SerializeField] float elasticity = 1;
    [SerializeField] bool snapping = false;

    public void Do(Transform element, in float duration, in Ease ease)
    {
        element.DOPunchPosition( punch, duration, vibrato, elasticity, snapping ).SetEase( ease );
    }
}
