using UnityEngine;

public abstract class GetGameObjectThen<T> : ITriggerable<T> where T : Component
{
    [SerializeReference,ExtendedDrawer] ITriggerable<GameObject>[] execute;

    public void Trigger(T t)
    {
        if( t == null ) return;
        execute.TriggerAll( t.gameObject );
    }
}
