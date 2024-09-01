using System.Collections;
using DG.Tweening;
using UnityEngine;

public interface ITween
{
    void Do( in float duration, in Ease ease );
}

public interface ITween<T> : ITween
{
}

public abstract class TweenOf<T> : ITween<T>
{
    [SerializeField] protected T[] elements;

    protected abstract void Do(T element, in float duration, in Ease ease);

    public void Do(in float duration, in Ease ease)
    {
        if( elements == null ) return;
        foreach( var element in elements )
        {
            if( element == null ) continue;
            Do( element, duration, ease );
        }
    }
}

public class DoTween : ITriggerable
{
    [SerializeField,InlineProperties] DoTweenBlend blend;
    [SerializeReference,ExtendedDrawer] ITween[] tweens;

    public void Trigger()
    {
        tweens.Do( blend );
    }
}

public interface ITweenAction<T>
{
    void Do( T element, in float duration, in Ease ease );
}

public abstract class DoOn<T> : ITween
{
    [SerializeField] T element;
    [SerializeReference,ExtendedDrawer] ITweenAction<T>[] tweens;

    public void Do(in float duration, in Ease ease)
    {
        tweens.Do( element, duration, ease );
    }
}

public abstract class DoOnArrayOf<T> : ITween
{
    [SerializeField] T[] elements;
    [SerializeReference,ExtendedDrawer] ITweenAction<T>[] tweens;

    public void Do(in float duration, in Ease ease)
    {
        tweens.Do( elements, duration, ease );
    }
}
