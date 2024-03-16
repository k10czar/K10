using UnityEngine;

public class ErrorSemaphore : ISemaphore
{
	public bool Free => true;
	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	public IEventRegister OnBlock => ErrorEvent.Ref;

	public IEventRegister OnRelease => ErrorEvent.Ref;

	public IEventRegister<bool> OnStateChange => ErrorEvent<bool>.Ref;

	public IEventRegister OnTrueState => ErrorEvent.Ref;

	public IEventRegister OnFalseState => ErrorEvent.Ref;

	public IBoolStateObserver Not => _not.Request( this );

	public bool Value => Free;

	public IEventRegister<bool> OnChange => ErrorEvent<bool>.Ref;
	
	public IEventValidator Validator => NullValidator.Instance;

	public void Interact( object obj, bool block ) => Debug.LogError( $"{obj} Interact a ERROR Semaphore" );
	public void Release( object obj ) => Debug.LogError( $"{obj} Release a ERROR Semaphore" );
	public bool Block( object obj, bool increaseBlock = true ) { Debug.LogError( $"{obj} Blocking {increaseBlock} a ERROR Semaphore" ); return false; }

	public int GetBlockCount(object key) => Free ? 0 : 1;

	public bool HasBlocker(object key) => !Free;
	public bool BlockButDoNotIncrease( object obj )
	{
		Debug.LogError( $"{obj} BlockButDoNotIncrease a ERROR Semaphore" );
		return true;
	}

	public bool Get() => Free;

	private ErrorSemaphore() { }

	[ConstLike] private static readonly ErrorSemaphore _instance = new ErrorSemaphore();
	public static ISemaphore Ref => _instance;
}
