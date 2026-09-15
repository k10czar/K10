using UnityEngine;

public class NamedSingletonComponent<T> where T : UnityEngine.Component
{
	private readonly AutoClearedReference<T> _instance = new AutoClearedReference<T>();
	private GameObject _cachedObject;
	private string _name;
	private IEventRegister _currentReferenceDestroyEvent;

	public NamedSingletonComponent( string objectAttachedName )
	{
		_name = objectAttachedName;
	}

	public T Instance
	{
		get
		{
			if( !_instance.IsValid )
			{
				if( _cachedObject == null )
					_cachedObject = GameObject.Find( _name );

				if( _cachedObject != null )
				{
					_instance.RegisterNewReference( _cachedObject.GetComponent<T>() );
					_currentReferenceDestroyEvent = _cachedObject.EventRelay().OnDestroy;
					_currentReferenceDestroyEvent.Register( OnDestroy );
				}
			}

			return _instance.Reference;
		}
	}

	void OnDestroy()
	{
		_cachedObject = null;
		_currentReferenceDestroyEvent.Unregister( OnDestroy );
		_currentReferenceDestroyEvent = null;
	}
}
