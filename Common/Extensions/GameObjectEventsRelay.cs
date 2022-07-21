using UnityEngine;

public interface IUnityEventsRelay
{
	IEventRegister OnDestroy { get; }
	IBoolStateObserver IsActive { get; }
	IBoolStateObserver IsAlive { get; }
	IEventValidator LifetimeValidator { get; }
}

public class GameObjectEventsRelay : MonoBehaviour, IUnityEventsRelay
{
	bool _destroyed = false;
	private EventSlot _onDestroy;
	private BoolState _isActive;
	private BoolState _isAlive;
	private ConditionalEventsCollection _lifetimeValidator;

	IEventRegister IUnityEventsRelay.OnDestroy => OnDestroyEvent;
	public IEventRegister OnDestroyEvent => _onDestroy ?? ( _onDestroy = new EventSlot() );
	public IBoolStateObserver IsActive => _isActive ?? ( _isActive = new BoolState( enabled ) );
	public IBoolStateObserver IsAlive => _isAlive ?? ( _isAlive = new BoolState( !_destroyed ) );
	public IEventValidator LifetimeValidator => _lifetimeValidator ?? NewLifetimeValidator();

	IEventValidator NewLifetimeValidator()
	{
		if( _destroyed ) return NullValidator.Instance;
		// Debug.Log( $"GameObjectEventsRelay.NewLifetime( {DebugName} ) => {GetStateDebug()}" );
		_lifetimeValidator = new ConditionalEventsCollection();
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
		_lifetimeValidator?.Void();

		_onDestroy?.Clear();
		_isActive?.Clear();
		_isAlive?.Clear();
		_lifetimeValidator?.Clear();

		_onDestroy = null;
		_isAlive = null;
		_isActive = null;
		_lifetimeValidator = null;
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
}
