using UnityEngine;
using BoolStateOperations;


public sealed class InputGroup : UpdaterOnDemand
{
	Semaphore _canUpdate = new Semaphore();
	public ISemaphore CanUpdate { get { return _canUpdate; } }

	OneTimeValidator _validator = new OneTimeValidator();
	public IEventValidator Validator => _validator;

	public InputGroup( MonoBehaviour behaviour, IEventValidator validator, InputGroup parent = null ) : base( behaviour, validator )
	{
		if( parent != null ) _canUpdate.ReleaseOn( parent._canUpdate, validator );
	}

	protected override void Kill()
	{
		base.Kill();
		_canUpdate?.Kill();
		_validator?.Kill();
	}

	// public void AddConstraint( InputGroup group ) { _canUpdate.ReleaseOn( group._canUpdate ); }
}
