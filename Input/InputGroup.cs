using UnityEngine;
using BoolStateOperations;


public sealed class InputGroup : UpdaterOnDemand
{
	Semaphore _canUpdate = new Semaphore();
	public ISemaphore CanUpdate { get { return _canUpdate; } }

	public InputGroup( MonoBehaviour behaviour, InputGroup parent = null ) : base( behaviour )
	{
		if( parent != null ) _canUpdate.ReleaseOn( parent._canUpdate );
	}

	// public void AddConstraint( InputGroup group ) { _canUpdate.ReleaseOn( group._canUpdate ); }
}
