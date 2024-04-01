using System.Collections.Generic;
using UnityEngine;

public abstract class KomposedDebugableMonoBehavior : MonoBehaviour, IDrawGizmos, IDrawGizmosOnSelected
{
    protected abstract IEnumerable<object> GetKomposedDebugableObjects();

    public IEnumerable<T> Foreach<T>()
    {
        foreach( var obj in GetKomposedDebugableObjects() ) if( obj is T t ) yield return t;
    }

#if UNITY_EDITOR
	public void OnDrawGizmos()
	{
        foreach( var dg in Foreach<IDrawGizmos>() ) dg.OnDrawGizmos();
	}

	public void OnDrawGizmosSelected()
	{
        foreach( var dg in Foreach<IDrawGizmosOnSelected>() ) dg.OnDrawGizmosSelected();
	}
#endif
}
