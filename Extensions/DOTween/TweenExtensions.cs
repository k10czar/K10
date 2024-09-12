using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class TweenExtensions
{
    public static void Do( this ITween tween, in DoTweenBlend blend, in float delay = 0 )
    {
        DoTweenBlend.GetEaseAndDuration( blend, out var duration, out var ease );
        tween.Do( duration, ease, delay );
    }

    public static void Do<T>( this IEnumerable<ITweenAction<T>> actions, T t, in float duration, in Ease ease, in float delay = 0 )
    {
        if( actions == null ) return;
        foreach( var act in actions )
        {
            if( act == null ) continue;
            var tween = act.Do( t, duration, ease );
            if( delay > Mathf.Epsilon ) tween.SetDelay( delay );
        }
    }

    public static void Do<T>( this IEnumerable<ITweenAction<T>> actions, T[] ts, in float duration, in Ease ease, in float delay = 0 )
    {
        if( actions == null ) return;
        foreach( var act in actions )
        {
            if( act == null ) continue;
            foreach( var t in ts )
            {
                if( t == null ) continue;
                var tween = act.Do( t, duration, ease );
                if( delay > Mathf.Epsilon ) tween.SetDelay( delay );
            }
        }
    }

    public static void Do( this IEnumerable<ITween> tweens, in DoTweenBlend blend, in float delay = 0 )
    {
        if( tweens == null ) return;
        DoTweenBlend.GetEaseAndDuration( blend, out var duration, out var ease );
        Do( tweens, duration, ease, delay );
    }

    public static void Do( this IEnumerable<ITween> tweens, in float duration, in Ease ease, in float delay = 0 )
    {
        if( tweens == null ) return;
        foreach( var t in tweens )
        {
            if( t == null ) continue;
            t.Do( duration, ease, delay );
        }
    }
}
