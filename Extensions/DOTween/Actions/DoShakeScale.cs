using DG.Tweening;
using UnityEngine;

public class DoShakeScale : ITweenAction<Transform>
{
    [SerializeField] float strength = 1;
    [SerializeField] int vibrato = 10;
    [SerializeField] float randomness = 90;
    [SerializeField] bool fadeOut = true;
    [SerializeField] ShakeRandomnessMode mode = ShakeRandomnessMode.Full;

    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        return element.DOShakeScale( duration, strength, vibrato, randomness, fadeOut, mode ).SetEase( ease );
    }
}
