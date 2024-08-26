using DG.Tweening;
using UnityEngine;

public class DoFadeLights : ITween
{
    [SerializeField] float value = 0;
    [SerializeField] Light[] lights;

    public void Do(in float duration, in Ease ease)
    {
        if( lights == null ) return;
        foreach( var light in lights )
        {
            light.DOIntensity( value, duration ).SetEase( ease );
        }
    }
}