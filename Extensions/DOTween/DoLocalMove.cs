using DG.Tweening;
using UnityEngine;

public class DoLocalMove : ITween
{
    [SerializeField] Vector3 value;
    [SerializeField] Transform[] transforms;

    public void Do(in float duration, in Ease ease)
    {
        if( transforms == null ) return;
        foreach( var t in transforms ) 
        {
            if( t == null ) continue;
            t.DOLocalMove( value, duration ).SetEase( ease );
        }
    }
}