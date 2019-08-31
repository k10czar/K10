using UnityEngine;

public class ErrorSemaphore : ISemaphore
{
	public bool Free => true;

	public IEventRegister OnBlock => ErrorEvent.Ref;

	public IEventRegister OnRelease => ErrorEvent.Ref;

	public IEventRegister<bool> OnStateChange => ErrorEvent<bool>.Ref;

	public IEventRegister OnTrueState => ErrorEvent.Ref;

	public IEventRegister OnFalseState => ErrorEvent.Ref;

	public bool Value => Free;

	public IEventRegister<bool> OnChange => ErrorEvent<bool>.Ref;

	public void Release( object obj ) => Debug.LogError( $"{obj} Release a ERROR Semaphore" );

	public void Block( object obj ) => Debug.LogError( $"{obj} Blocking a ERROR Semaphore" );

	public bool BlockButDoNotIncrease( object obj )
	{
		Debug.LogError( $"{obj} BlockButDoNotIncrease a ERROR Semaphore" );
		return true;
	}

	public bool Get() => Free;

	private ErrorSemaphore() { }

	private static readonly ErrorSemaphore _instance = new ErrorSemaphore();
	public static ISemaphore Ref => _instance;
}
