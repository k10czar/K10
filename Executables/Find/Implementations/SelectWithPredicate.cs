using UnityEngine;
using System.Collections.Generic;

public abstract class SelectWithPredicate<T,K> : IFinder<T,K>
{
    [SerializeField] int maxCount = -1;
    
    System.Func<K,bool> predicateInstance;
    public System.Func<K, bool> PredicateInstance
    {
        get
        {
            if( predicateInstance == null ) predicateInstance = Predicate;
            return predicateInstance;
        }
    }

    public abstract IEnumerable<K> GetEnumeration( T t );

    protected abstract bool Predicate( K k );
    public IEnumerator<K> Find( T t ) 
    {
        var predicate = PredicateInstance;
        var count = 0;
        if( maxCount != 0 )
        {
            foreach( var m in GetEnumeration( t ) )
            {
                if( predicate( m ) ) 
                {
                    count++;
                    yield return m;
                    if( count == maxCount ) break;
                }
            }
        }
    }
}
