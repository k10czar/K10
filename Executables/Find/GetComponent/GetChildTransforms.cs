using System.Collections.Generic;
using UnityEngine;

public class GetChildTransforms : IFinder<GameObject, Transform>
{
    public IEnumerator<Transform> Find(GameObject go)
    {
        if( go != null )
        {
            foreach( Transform child in go.transform ) 
                if( child != null ) 
                    yield return child;
        }
    }
}
