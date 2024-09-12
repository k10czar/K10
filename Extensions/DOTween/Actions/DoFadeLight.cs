using DG.Tweening;
using UnityEngine;

public class DoFadeLight : ITweenAction<Light>
{
    [SerializeField] float value = 0;

    public Tweener Do(Light element, in float duration, in Ease ease)
    {
        return element.DOIntensity( value, duration ).SetEase( ease );
    }
}