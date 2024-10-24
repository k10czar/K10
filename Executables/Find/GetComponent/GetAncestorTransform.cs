using System.Collections.Generic;
using UnityEngine;

public class GetAncestorTransform : IFinder<GameObject, Transform>
{
    public IEnumerator<Transform> Find(GameObject go)
    {
        if( go != null && go.transform != null )
        {
            var ancestor = go.transform.Ancestor();
            if( ancestor != null ) yield return ancestor;
        }
    }
}
