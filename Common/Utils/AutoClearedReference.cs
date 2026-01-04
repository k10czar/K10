using UnityEngine;

public class AutoClearedReference<T> where T : UnityEngine.Component
{
	private T _reference;
	public T Reference => _reference;

	// string debugName = "NOT_INITIALIZED";

	private IVoidable _currentRefClear;

	public bool IsValid => _reference != null;

	public void RegisterNewReference( T newRef )
	{
		if( _reference == newRef ) return;
		
		_currentRefClear?.Void();
		_currentRefClear = null;

		_reference = newRef;
		// debugName = $"{newRef.NameOrNull()}({typeof( T )})";
		// Debug.Log( $"--->AutoClearedReference.RegisterNewReference( {debugName} )" );

		if( _reference == null ) return;

		var voidEvent = new VoidableCallOnce( OnReferenceDestroy );
		_reference.gameObject.EventRelay().OnDestroy.Register( voidEvent );
		_currentRefClear = voidEvent;
	}

	private void OnReferenceDestroy()
	{
		// Debug.Log( $"--->AutoClearedReference.OnReferenceDestroy( {debugName} )" );
		_currentRefClear = null;
		_reference = null;
	}
}
