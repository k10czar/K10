using System.Collections.Generic;
using UnityEngine;

public class GetTransform : IFinder<GameObject, Transform>
{
    public IEnumerator<Transform> Find(GameObject go)
    {
        if( go != null )
        {
            if( go.transform != null ) yield return go.transform;
        }
    }
}
