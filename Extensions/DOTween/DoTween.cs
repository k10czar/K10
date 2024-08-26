using DG.Tweening;
using UnityEngine;

public interface ITween
{
    void Do( in float duration, in Ease ease );
}

public static class TweenExtensions
{
    public static void Do( this ITween tween, in DoTweenBlend blend )
    {
        DoTweenBlend.GetBlendAndDuration( blend, out var duration, out var ease );
        tween.Do( duration, ease );
    }
}

public class DoTween : ITriggerable
{
    [SerializeField,InlineProperties] DoTweenBlend blend;
    [SerializeReference,ExtendedDrawer] ITween[] tweens;

    public void Trigger()
    {
        if( tweens == null ) return;
        DoTweenBlend.GetBlendAndDuration( blend, out var duration, out var ease );
        foreach( var t in tweens )
        {
            if( t == null ) continue;
            t.Do( duration, ease );
        }
    }
}
