using UnityEngine;


public interface IReferenceRequester<T>
{
	void ClearOverrides();
	void RemoveRequest( object key );
	void RequestOverride( T value, object key );
	void ChangeDefault( T value );
}

[System.Serializable]
public class ReferenceOverrideRequester<T> : IReferenceHolder<T>, IReferenceRequester<T>
{
	[SerializeField] private T _default = default( T );
	[SerializeField] private readonly CachedReference<T> _current = new CachedReference<T>();

	[SerializeField] private readonly System.Collections.Generic.List<Request> _requests = new System.Collections.Generic.List<Request>();

	public T CurrentReference => _current.CurrentReference;
	public IEventRegister<T, IEventValidator> OnReferenceSet => _current.OnReferenceSet;
	public IEventRegister<T> OnReferenceRemove => _current.OnReferenceRemove;
	public IEventValidator Validator => _current.Validator;
	public bool IsNull => _current.IsNull;

	public T CurrentOriginalValue => _default;
	public bool HasAnyOverride => _requests.Count > 0;
	public bool IsOverrideSameAsOriginal => Algorithm.SafeEquals(_current.CurrentReference, _default);

	public ReferenceOverrideRequester( T defaultValue = default(T) )
	{
		ChangeDefault( defaultValue );
	}

	public void ClearOverrides()
	{
		_requests.Clear();
		_current.ChangeReference( _default );
	}

	public void RemoveRequest( object key )
	{
		RemoveAllRequests( key );
		UpdateReference();
	}

	private void RemoveAllRequests( object key )
	{
		for( int i = _requests.Count - 1; i >= 0; i-- )
		{
			if( _requests[i].Key != key ) continue;
			_requests.RemoveAt( i );
		}
	}

	public void RequestOverride( T value, object key )
	{
		RemoveAllRequests( key );
		_requests.Add( new Request( key, value ) );
		_current.ChangeReference( value );
	}

	public void ChangeDefault( T value )
	{
		_default = value;
		if( _requests.Count == 0 ) _current.ChangeReference( value );
	}

	private void UpdateReference()
	{
		T newRef = default( T );
		if( _requests.Count == 0 ) newRef = _default; 
		else newRef = _requests[_requests.Count - 1].Value;
		_current.ChangeReference( newRef );
	}

	public override string ToString() => $"{{ {string.Join( ", ", _requests.ConvertAll( ( t ) => t.ToStringOrNull() ) )} }}";

	public class Request
	{
		private readonly object _key;
		public readonly T _value;

		public object Key => _key;
		public T Value => _value;

		public Request( object key, T value )
		{
			_key = key;
			_value = value;
		}

		public override string ToString() => $"[ {_key}: {_value} ]";
	}
}