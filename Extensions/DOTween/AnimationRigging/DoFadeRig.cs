using DG.Tweening;
using UnityEngine;

public class DoFadeRig : ITween
{
    [SerializeField,Range(0, 1)] float value = 0;
    [SerializeField] UnityEngine.Animations.Rigging.Rig[] rigs;

    public void Do(in float duration, in Ease ease)
    {
        if( rigs == null ) return;
        foreach( var rig in rigs )
        {
            rig.DOFade( value, duration ).SetEase( ease );
        }
    }
}

