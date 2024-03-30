using UnityEngine;

public interface IUnityEventsRelay
{
	IEventRegister OnDestroy { get; }
	// IEventRegister OnLateDestroy { get; }
	IBoolStateObserver IsActive { get; }
	IBoolStateObserver IsAlive { get; }
	IEventValidator LifetimeValidator { get; }
}

public class GameObjectEventsRelay : MonoBehaviour, IUnityEventsRelay
{
	bool _destroyed = false;
	private EventSlot _onDestroy;
	// private EventSlot _onLateDestroy;
	private BoolState _isActive;
	private BoolState _isAlive;
	private Validator _lifetimeValidator;

	IEventRegister IUnityEventsRelay.OnDestroy => OnDestroyEvent;
	// public IEventRegister OnLateDestroy => Lazy.Request( ref _onLateDestroy );
	public IEventRegister OnDestroyEvent => Lazy.Request( ref _onDestroy );
	public IBoolStateObserver IsActive
	{
		get
		{
			if( _isActive == null ) _isActive = new BoolState( enabled && gameObject.activeInHierarchy );
			return _isActive;
		}
	}

	public IBoolStateObserver IsAlive
	{
		get
		{
			if( _isAlive == null ) 
			{
				if( _destroyed ) return FalseState.Instance;
				_isAlive = new BoolState( true );
			}
			return _isAlive;
		}
	}

	public IEventValidator LifetimeValidator => _lifetimeValidator ?? NewLifetimeValidator();

	IEventValidator NewLifetimeValidator()
	{
		if( _destroyed ) return NullValidator.Instance;
		// Debug.Log( $"GameObjectEventsRelay.NewLifetime( {DebugName} ) => {GetStateDebug()}" );
		_lifetimeValidator = new Validator( this );
		return _lifetimeValidator;
	}

	// string _debugName;
	// static int _count = 0;
	// string DebugName 
	// {
	// 	get
	// 	{
	// 		if( _debugName == null )
	// 		{
	// 			_debugName = _count.ToString() + ")" + this.HierarchyNameOrNull();
	// 			_count++;
	// 		}
	// 		return _debugName;
	// 	}
	// }

	// void Awake() { Debug.Log( $"GameObjectEventsRelay.Awake( {DebugName} ) => {GetStateDebug()}" ); }
	// void Start() { Debug.Log( $"GameObjectEventsRelay.Start( {DebugName} ) => {GetStateDebug()}" ); }

	void OnDestroy()
	{
		// Debug.Log( $"GameObjectEventsRelay.OnDestroy( {DebugName} ) => {GetStateDebug()}" );
		Clear();
	}

	void Clear()
	{
		_destroyed = true;
		_onDestroy?.Trigger();
		_isAlive?.SetFalse();
		_lifetimeValidator?.OnDestroy();

		GcClear.AfterKill( ref _onDestroy );
		GcClear.AfterKill( ref _isAlive );
		GcClear.AfterKill( ref _isActive );
		GcClear.AfterKill( ref _lifetimeValidator );

		// _onLateDestroy?.Trigger();
		// GcClear.AfterKill( ref _onLateDestroy );
	}

	void OnEnable() => _isActive?.SetTrue();
	void OnDisable()
	{
		// Debug.Log( $"GameObjectEventsRelay.OnDisable( {DebugName} ) => {GetStateDebug()}" );
		_isActive?.SetFalse();
	}

	~GameObjectEventsRelay() 
	{
		// Debug.Log( $"~GameObjectEventsRelay.Destructor( {DebugName} ) => {GetStateDebug()}" );
		Clear();
	}

	// string GetStateDebug() => $"{( ( _onDestroy != null ) ? "oD" : "" )}{( ( _isActive != null ) ? "aC" : "" )}{( ( _isAlive != null ) ? "aL" : "" )}{( ( _lifetimeValidator != null ) ? "lT" : "" )}";

	private class Validator : IEventValidator, ICustomDisposableKill
	{
		GameObjectEventsRelay _objRelay;

		bool _destroyed = false;
		System.Func<bool> _currentValidationCheck;
		EventSlot _onVoid;

		bool _lastValidation = true;

		public IEventRegister OnVoid => Lazy.Request( ref _onVoid );

		public Validator( GameObjectEventsRelay objRelay )
		{
			_objRelay = objRelay;
		}

		private bool ValidationCheck()
		{
			if( !_lastValidation ) return false;
			_lastValidation = _objRelay != null && _objRelay.transform != null && !_destroyed;
			if( !_lastValidation ) OnDestroy();
			return _lastValidation;
		}

		public System.Func<bool> CurrentValidationCheck => _currentValidationCheck ?? ( _currentValidationCheck = ValidationCheck );

		public void Kill()
		{
			_onVoid?.Kill();
			_onVoid = null;
		}

		public void OnDestroy()
		{
			_destroyed = true;
			_lastValidation = false;
			_onVoid?.Trigger();
		}
	}
}
