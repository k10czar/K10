using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DoFadeRig : ITweenAction<Rig>
{
    [SerializeField,Range(0, 1)] float value = 0;

    public Tweener Do(Rig element, in float duration, in Ease ease)
    {
        return element.DOFade( value, duration ).SetEase( ease );
    }
}