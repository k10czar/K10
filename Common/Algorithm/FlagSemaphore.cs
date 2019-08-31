using System;
using System.Collections.Generic;


public interface IFlagSemaphore
{
	IEventRegister<string, bool> OnChangeState { get; }
	void Lock( string lockName, object activator );
	void Release( string lockName, object releaser );
	bool Locked( string lockName );
	void Clear();
}

public class FlagSemaphore : IFlagSemaphore
{
	Dictionary<string, List<object>> _activators = new Dictionary<string, List<object>>();

	EventSlot<string, bool> _changeStateEvent = new EventSlot<string, bool>();
	public IEventRegister<string, bool> OnChangeState { get { return _changeStateEvent; } }

	public FlagSemaphore() { }
	public FlagSemaphore( Action<string, bool> changeAct )
	{
		_changeStateEvent.Register( changeAct );
	}

	public bool Locked( string lockName ) { return _activators.ContainsKey( lockName ); }

	public void Lock( string lockName, object activator )
	{
		List<object> acts;
		if( !_activators.TryGetValue( lockName, out acts ) )
		{
			acts = new List<object>();
			_activators[lockName] = acts;
		}

		if( !acts.Contains( activator ) )
		{
			if( acts.Count == 0 )
				_changeStateEvent.Trigger( lockName, true );

			acts.Add( activator );
		}
	}

	public void Release( string lockName, object releaser )
	{
		List<object> acts;
		if( _activators.TryGetValue( lockName, out acts ) && acts.Contains( releaser ) )
		{
			acts.Remove( releaser );

			if( acts.Count == 0 )
			{
				_changeStateEvent.Trigger( lockName, false );
				_activators.Remove( lockName );
            }
		}
	}

	public void Clear()
	{
		foreach( var kvp in _activators )
		{
			var key = kvp.Key;
			var list = kvp.Value;
			list.Clear();
			_changeStateEvent.Trigger( key, false );
		}
		_activators.Clear();
    }
}