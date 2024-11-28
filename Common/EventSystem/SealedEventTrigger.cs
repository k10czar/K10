

[UnityEngine.HideInInspector]
public sealed class SealedTrigger :  IEventTrigger
{
	System.Action _callback;
	bool _valid = true;

	public SealedTrigger( System.Action callback ) { _callback = callback; }
	public void Trigger() { if( _valid ) _callback(); _valid = false; }
	public bool IsValid { get { return _valid; } }
}

[UnityEngine.HideInInspector]
public sealed class SealedTrigger<T> : IEventTrigger<T>
{
	System.Action<T> _callback;
	bool _valid = true;

	public SealedTrigger( System.Action<T> callback ) { _callback = callback; }
	public void Trigger( T t ) { if( _valid ) _callback( t ); _valid = false; }
	public bool IsValid { get { return _valid; } }
}

[UnityEngine.HideInInspector]
public sealed class SealedTrigger<T,K> : IEventTrigger<T,K>
{
	System.Action<T, K> _callback;
	bool _valid = true;

	public SealedTrigger( System.Action<T,K> callback ) { _callback = callback; }
	public void Trigger( T t, K k ) { if( _valid ) _callback( t, k ); _valid = false; }
	public bool IsValid { get { return _valid; } }
}