using System.Collections.Generic;
using UnityEngine;

public class GetComponentStrategy<T> : IFinder<GameObject, T> where T : Component
{
    public IEnumerator<T> Find(GameObject go)
    {
        if( go != null )
        {
            var comp = go.GetComponent<T>();
            if( comp != null ) yield return comp;
        }
    }
}