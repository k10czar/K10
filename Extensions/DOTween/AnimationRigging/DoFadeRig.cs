using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DoFadeRig : ITweenAction<Rig>
{
    [SerializeField,Range(0, 1)] float value = 0;

    public void Do(Rig element, in float duration, in Ease ease)
    {
        element.DOFade( value, duration ).SetEase( ease );
    }
}