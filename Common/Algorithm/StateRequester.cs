using UnityEngine;

public interface IStateRequesterInfo
{
	bool Requested { get; }

	IEventRegister<bool> OnStateChange { get; }
	IEventRegister OnIgnore { get; }
	IEventRegister OnRequest { get; }
	IEventRegister OnInteraction { get; }

	int GetRequestCount(object key);
	bool HasRequest(object key);
}

public interface IStateRequesterInteraction
{
	void Toggle( object obj );
	bool Request( object obj, bool increaseRequest = true );
	void Clear();
	bool RequestButDoNotIncrease( object obj );
	void RemoveRequest( object obj );
	void RequestOn( IBoolStateObserver source );
	void IgnoreOn( IBoolStateObserver source );
	void RequestOn( IBoolStateObserver source, IEventValidator validator );
	void RequestOn(IBoolStateObserver source, IEventValidator validator, string nameGameObjectDebug);
	void IgnoreOn( IBoolStateObserver source, IEventValidator validator );
	void RequestOn( GameObject gameObject, IBoolStateObserver additionalCondition = null );
	void IgnoreOn( GameObject gameObject, IBoolStateObserver additionalCondition = null );
}

public interface IStateRequester : IStateRequesterInfo, IStateRequesterInteraction, IBoolStateObserver { }

public class StateRequester : IStateRequester, ICustomDisposableKill
{
	private bool _killed = false;


	// TODO: LazyOptimization
	private Semaphore _semaphore;
	private EventSlot<bool> _invertedSemaphoreStateChange;
	// EventSlot<bool> _invertedSemaphoreStateChange = new EventSlot<bool>();
	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	private Semaphore SemaphoreToInvert
	{
		get
		{
			if( _killed ) return _semaphore;
			return Lazy.Request( ref _semaphore );
		}
	}

	public bool Requested { get { return ( !_semaphore?.Free ) ?? false; } }

	public IEventRegister<bool> OnStateChange => OnChange;
	public IEventRegister OnIgnore { get { return SemaphoreToInvert.OnRelease; } }
	public IEventRegister OnRequest { get { return SemaphoreToInvert.OnBlock; } }

	public IBoolStateObserver Not => _not.Request( this );

	public bool Value { get { return Requested; } }
	public bool Get() { return Requested; }
	public IEventRegister<bool> OnChange 
	{
		get
		{
			if( !_killed && _invertedSemaphoreStateChange == null )
			{
				_invertedSemaphoreStateChange = new EventSlot<bool>();
				SemaphoreToInvert.OnStateChange.Register( InvertedStateChangeRelay );
			}
			return _invertedSemaphoreStateChange;
		}
	} 
	public IEventRegister OnTrueState { get { return SemaphoreToInvert.OnBlock; } }
	public IEventRegister OnFalseState { get { return SemaphoreToInvert.OnRelease; } }
	public IEventRegister OnInteraction => SemaphoreToInvert.OnInteraction;

	public void RequestOn( IBoolStateObserver source ) { SemaphoreToInvert.BlockOn( source ); }
	public void IgnoreOn( IBoolStateObserver source ) { SemaphoreToInvert.ReleaseOn( source ); }

	public void RequestOn( GameObject gameObject, IBoolStateObserver additionalCondition = null ) { SemaphoreToInvert.BlockOn( gameObject, additionalCondition ); }
	public void IgnoreOn( GameObject gameObject, IBoolStateObserver additionalCondition = null ) { SemaphoreToInvert.ReleaseOn( gameObject, additionalCondition ); }

	public void RequestOn( IBoolStateObserver source, IEventValidator validator ) { SemaphoreToInvert.BlockOn( source, validator ); }

	public void RequestOn(IBoolStateObserver source, IEventValidator validator, string nameGameObjctToDebug) { SemaphoreToInvert.BlockOn(source, validator, nameGameObjctToDebug); }
	public void IgnoreOn( IBoolStateObserver source, IEventValidator validator ) { SemaphoreToInvert.ReleaseOn( source, validator ); }

	void InvertedStateChangeRelay( bool free ) { _invertedSemaphoreStateChange?.Trigger( !free ); }

	public void Clear() { _semaphore?.Clear(); }

	public void Toggle( object obj ) { SemaphoreToInvert.Toggle( obj ); }
	public bool Request( object obj, bool increaseRequest = true ) => SemaphoreToInvert.Block( obj, increaseRequest );
	public bool RequestButDoNotIncrease( object obj ) => Request( obj, false );
	public void RemoveRequest( object obj ) => _semaphore?.Release( obj );

	public int GetRequestCount( object key ) => _semaphore?.GetBlockCount( key ) ?? 0;

	public bool HasRequest( object key ) => _semaphore?.HasBlocker( key ) ?? false;

	public override string ToString() => $"( {( Requested ? "Requested" : "False" )} State => {{ !!{_semaphore.ToStringOrNull()}!! }} )";

	public void Kill()
	{
		_killed = true;
		_semaphore?.Kill();
		_invertedSemaphoreStateChange?.Kill();
	}
}
