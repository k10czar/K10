using UnityEngine;

public interface IStateRequesterInfo
{
	bool Requested { get; }

	IEventRegister<bool> OnStateChange { get; }
	IEventRegister OnIgnore { get; }
	IEventRegister OnRequest { get; }
}

public interface IStateRequesterInteraction
{
	void Toggle( object obj );
	void Request( object obj );
	bool RequestButDoNotIncrease( object obj );
	void RemoveRequest( object obj );
	void RequestOn( IBoolStateObserver source );
	void IgnoreOn( IBoolStateObserver source );
}

public interface IStateRequester : IStateRequesterInfo, IStateRequesterInteraction, IBoolStateObserver { }

public class StateRequester : IStateRequester
{
	private readonly Semaphore _semaphore = new Semaphore();

	public bool Requested { get { return !_semaphore.Free; } }

	EventSlot<bool> _onStateChange = new EventSlot<bool>();

	public IEventRegister<bool> OnStateChange { get { return _onStateChange; } }
	public IEventRegister OnIgnore { get { return _semaphore.OnRelease; } }
	public IEventRegister OnRequest { get { return _semaphore.OnBlock; } }

	public bool Value { get { return Requested; } }
	public bool Get() { return Requested; }
	public IEventRegister<bool> OnChange { get { return _onStateChange; } }
	public IEventRegister OnTrueState { get { return _semaphore.OnBlock; } }
	public IEventRegister OnFalseState { get { return _semaphore.OnRelease; } }

	public StateRequester() { _semaphore.OnStateChange.Register( InvertedStateChange ); }

	public void RequestOn( IBoolStateObserver source ) { _semaphore.BlockOn( source ); }
	public void IgnoreOn( IBoolStateObserver source ) { _semaphore.ReleaseOn( source ); }

	void InvertedStateChange( bool free ) { _onStateChange.Trigger( !free ); }

	public void Clear() { _semaphore.Clear(); }

	public void Toggle( object obj ) { _semaphore.Toggle( obj ); }
	public void Request( object obj ) { _semaphore.Block( obj ); }
	public bool RequestButDoNotIncrease( object obj ) => _semaphore.BlockButDoNotIncrease( obj );
	public void RemoveRequest( object obj ) { _semaphore.Release( obj ); }

	public override string ToString() => $"( {( Requested ? "Requested" : "False" )} State => {{ !!{_semaphore}!! }} )";
}