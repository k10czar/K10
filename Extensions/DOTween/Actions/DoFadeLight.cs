using DG.Tweening;
using UnityEngine;

public class DoFadeLight : ITweenAction<Light>
{
    [SerializeField] float value = 0;

    public void Do(Light element, in float duration, in Ease ease)
    {
        element.DOIntensity( value, duration ).SetEase( ease );
    }
}