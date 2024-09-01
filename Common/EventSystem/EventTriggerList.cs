using System.Collections.Generic;


[UnityEngine.HideInInspector]
public class EventTriggerList : IEventTrigger
{
	List<IEventTrigger> _triggers = new List<IEventTrigger>();
	
	public EventTriggerList( List<IEventTrigger> triggers ) { _triggers.AddRange( triggers ); }

	public void Trigger() { foreach( var t in _triggers ) { t.Trigger(); } }
	bool IValidatedObject.IsValid { get { return true; } }
}

[UnityEngine.HideInInspector]
public class EventTriggerList<T> : IEventTrigger<T>
{
	List<IEventTrigger<T>> _triggers = new List<IEventTrigger<T>>();
	
	public EventTriggerList( List<IEventTrigger<T>> triggers ) { _triggers.AddRange( triggers ); }

	public void Trigger( T t ) { foreach( var tgr in _triggers ) { tgr.Trigger( t ); } }
	bool IValidatedObject.IsValid { get { return true; } }
}

[UnityEngine.HideInInspector]
public class EventTriggerList<T,K> : IEventTrigger<T,K>
{
	List<IEventTrigger<T,K>> _triggers = new List<IEventTrigger<T,K>>();
	
	public EventTriggerList( List<IEventTrigger<T,K>> triggers ) { _triggers.AddRange( triggers ); }

	public void Trigger( T t, K k ) { foreach( var tgr in _triggers ) { tgr.Trigger( t, k ); } }
	bool IValidatedObject.IsValid { get { return true; } }
}