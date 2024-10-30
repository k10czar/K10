using System.Collections.Generic;
using UnityEngine;

public class GetComponentsTypeStrategy<T> : IFinder<GameObject, T>
{
    public IEnumerator<T> Find(GameObject go)
    {
        if( go != null )
        {
            var comp = go.GetComponents( typeof( T ) );
            if( comp != null && comp is T t ) yield return t;
        }
    }
}
