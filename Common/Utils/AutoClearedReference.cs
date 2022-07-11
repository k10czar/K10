public class AutoClearedReference<T> where T : UnityEngine.Component
{
	private T _reference;
	public T Reference => _reference;

	private IEventRegister _currentReferenceDestroyEvent;

	public bool IsValid => _reference != null;

	public void RegisterNewReference( T newRef )
	{
		if( _reference != newRef ) OnReferenceDestroy();

		_reference = newRef;

		if( _reference == null ) return;

		_currentReferenceDestroyEvent = _reference.gameObject.EventRelay().OnDestroy;
		_currentReferenceDestroyEvent.Register( OnReferenceDestroy );
	}

	private void OnReferenceDestroy()
	{
		_reference = null;
		_currentReferenceDestroyEvent?.Unregister( OnReferenceDestroy );
		_currentReferenceDestroyEvent = null;
	}
}
