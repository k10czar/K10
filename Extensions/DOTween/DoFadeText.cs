using DG.Tweening;
using TMPro;
using UnityEngine;

public class DoFadeText : ITween
{
    [SerializeField,Range(0,1)] float value = 0;
    [SerializeField] TMP_Text[] texts;

    public void Do(in float duration, in Ease ease)
    {
        if( texts == null ) return;
        foreach( var text in texts )
        {
            if( text == null ) continue;
            text.DOFade( value, duration ).SetEase( ease );
        }
    }
}
