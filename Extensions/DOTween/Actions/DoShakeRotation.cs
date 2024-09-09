using DG.Tweening;
using UnityEngine;

public class DoShakeRotation : ITweenAction<Transform>
{
    [SerializeField] float strength = 1;
    [SerializeField] int vibrato = 10;
    [SerializeField] float randomness = 90;
    [SerializeField] bool fadeOut = true;
    [SerializeField] ShakeRandomnessMode mode = ShakeRandomnessMode.Full;

    public void Do(Transform element, in float duration, in Ease ease)
    {
        element.DOShakeRotation( duration, strength, vibrato, randomness, fadeOut, mode ).SetEase( ease );
    }
}
