using DG.Tweening;
using TMPro;
using UnityEngine;

public class DoFadeText : ITweenAction<TMP_Text>
{
    [SerializeField,Range(0,1)] float value = 0;

    public void Do(TMP_Text element, in float duration, in Ease ease)
    {
        element.DOFade( value, duration ).SetEase( ease );
    }
}