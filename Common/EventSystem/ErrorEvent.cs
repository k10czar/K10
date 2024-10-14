using System;
using UnityEngine;

[UnityEngine.HideInInspector]
public class ErrorEvent : IEvent
{
	public bool IsValid => true;

	public void Register( IEventTrigger listener ) => Debug.LogError( $"{listener} Register in a ERROR event" );

	public void Trigger() => Debug.LogError( "ERROR event has been triggered" );

	public bool Unregister( IEventTrigger listener ) { Debug.LogError( $"{listener} Unregister in a ERROR event" ); return true; }

	private ErrorEvent() { }

	private static readonly ErrorEvent _instance = new ErrorEvent();
	public static IEvent Ref => _instance;
}

[UnityEngine.HideInInspector]
public class ErrorEvent<T> : IEvent<T>
{
	public bool IsValid => true;

	public void Register( IEventTrigger listener ) => Debug.LogError( $"{listener} Register in a ERROR event" );
	public void Register( IEventTrigger<T> listener ) => Debug.LogError( $"{listener} Register in a ERROR event" );

	public void Trigger( T t ) => Debug.LogError( $"ERROR event has been triggered with parameter T({t})" );

	public bool Unregister( IEventTrigger listener ) { Debug.LogError( $"{listener} Unregister in a ERROR event" ); return true; }
	public bool Unregister( IEventTrigger<T> listener ) { Debug.LogError( $"{listener} Unregister in a ERROR event" ); return true; }

	private ErrorEvent() { }

	private static readonly ErrorEvent<T> _instance = new ErrorEvent<T>();
	public static IEvent<T> Ref => _instance;
}
