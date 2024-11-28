using UnityEngine;

public class FindAndExecute<T,K> : ITriggerable<T>
{
    [SerializeReference,ExtendedDrawer] IFinder<T,K> finder;
    [SerializeReference,ExtendedDrawer] ITriggerable<K>[] execute;

    public void Trigger( T r )
    {
        if( r == null ) return;
        var elements = finder.Find( r );
        if( elements == null ) return;
        while( elements.MoveNext() )
        {
            execute.TriggerAll( elements.Current );
        }
    }
}
