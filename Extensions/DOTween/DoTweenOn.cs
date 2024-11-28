using UnityEngine;

public abstract class DoTweenOn<T> : ITriggerable<T>
{
    [SerializeField,InlineProperties] DoTweenBlend blend;
    [SerializeField,Unit("Seconds")] float delay = 0;
    [SerializeReference,ExtendedDrawer] ITweenAction<T>[] tweens;

    public void Trigger( T t )
    {
        DoTweenBlend.GetEaseAndDuration( blend, out var duration, out var ease );
        tweens.Do( t, duration, ease, delay );
    }
}
