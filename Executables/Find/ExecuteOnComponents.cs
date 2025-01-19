using UnityEngine;

public class ExecuteOnComponents<T> : ITriggerable<GameObject> where T : Component
{
    [SerializeReference,ExtendedDrawer] IFinder<GameObject,T> findStrategy;
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] execute;

    public void Trigger(GameObject obj)
    {
        if (obj == null) return;
        if (findStrategy == null) return;
        var components = findStrategy.Find( obj );
        if (components == null) return;
        while( components.MoveNext() )
        {
            execute.TriggerAll( components.Current );
        }
    }
}
