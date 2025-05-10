using UnityEngine;

public class OnObjectDestroy : IMoment
{
	[SerializeField] GameObject _gameObject;
	public IEventRegister GetEvent() => _gameObject.EventRelay().OnDestroy;
}