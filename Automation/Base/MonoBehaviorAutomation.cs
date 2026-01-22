

using System.Collections;
using K10.Automation;
using UnityEngine;

public abstract class MonoBehaviorAutomation<T> : BaseOperation where T : MonoBehaviour
{        
    public virtual bool WaitForActiveAndEnable => true;
    public virtual float WaitTimeForSearch => .1f;
    public virtual FindObjectsInactive SearchParam => FindObjectsInactive.Include;
    
    public abstract IEnumerator ExecuteOnMonoBehaviour( T t, bool log = false );
    
    public override IEnumerator ExecutionCoroutine( bool log = false ) 
    {
        T mb = null;
        int searchCount = 0;
        var wait = new WaitForSeconds( WaitTimeForSearch );
        while( mb == null ) 
        {
            mb = Object.FindAnyObjectByType<T>( SearchParam );
            searchCount++;
            if( log ) Debug.Log( $"Trying to Find {typeof(T)} searchCount:{searchCount}" );
            if( mb == null ) yield return wait;
        }
        if( log ) Debug.Log( $"Found {typeof(T)} in {mb.HierarchyNameOrNull()}" );
        if( WaitForActiveAndEnable ) 
        {
            if( log && mb.isActiveAndEnabled ) Debug.Log( $"Waiting for {mb.HierarchyNameOrNull()} be active" );
            while( !mb.isActiveAndEnabled ) yield return wait;
            if( log ) Debug.Log( $"Waiting for {mb.HierarchyNameOrNull()} is active" );
        }
        yield return ExecuteOnMonoBehaviour( mb, log );
    }
}