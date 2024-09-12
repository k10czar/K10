using System.Collections;
using DG.Tweening;
using UnityEngine;

public interface ITween
{
    void Do( in float duration, in Ease ease, in float delay = 0 );
}

public interface ITween<T> : ITween
{
}

public abstract class TweenOf<T> : ITween<T>
{
    [SerializeField] protected T[] elements;

    protected abstract void Do(T element, in float duration, in Ease ease, in float delay = 0);

    public void Do(in float duration, in Ease ease, in float delay = 0)
    {
        if( elements == null ) return;
        foreach( var element in elements )
        {
            if( element == null ) continue;
            Do( element, duration, ease, delay );
        }
    }
}

public class DoTween : ITriggerable
{
    [SerializeField,InlineProperties] DoTweenBlend blend;
    [SerializeField,Unit("Seconds")] float delay = 0;
    [SerializeReference,ExtendedDrawer] ITween[] tweens;

    public void Trigger()
    {
        tweens.Do( blend, delay );
    }
}

public interface ITweenAction<T>
{
    Tweener Do( T element, in float duration, in Ease ease );
}

public abstract class DoOn<T> : ITween
{
    [SerializeField] T element;
    [SerializeReference,ExtendedDrawer] ITweenAction<T>[] tweens;

    public void Do(in float duration, in Ease ease, in float delay = 0)
    {
        tweens.Do( element, duration, ease, delay );
    }
}

public abstract class DoOnArrayOf<T> : ITween
{
    [SerializeField] T[] elements;
    [SerializeReference,ExtendedDrawer] ITweenAction<T>[] tweens;

    public void Do(in float duration, in Ease ease, in float delay = 0)
    {
        tweens.Do( elements, duration, ease, delay );
    }
}
