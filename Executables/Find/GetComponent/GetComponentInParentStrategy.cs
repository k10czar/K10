using System.Collections.Generic;
using UnityEngine;

public class GetComponentInParentStrategy<T> : IFinder<GameObject, T> where T : Component
{
    public IEnumerator<T> Find(GameObject go)
    {
        if( go != null )
        {
            var comp = go.GetComponentInParent<T>();
            if( comp != null ) yield return comp;
        }
    }
}
