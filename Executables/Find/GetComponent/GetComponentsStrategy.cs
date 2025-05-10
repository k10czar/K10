using System.Collections.Generic;
using UnityEngine;

public class GetComponentsStrategy<T> : IFinder<GameObject, T> where T : Component
{
    public IEnumerator<T> Find(GameObject go)
    {
        if( go != null )
        {
            foreach( var comp in go.GetComponents<T>() )
            {
                if( comp != null ) yield return comp;
            }
        }
    }
}
