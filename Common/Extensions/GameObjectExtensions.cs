using UnityEngine;
public static class GameObjectExtensions
{
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
		public IEventValidator LifetimeValidator => _lifetimeValidator ?? ( _lifetimeValidator = new ConditionalEventsCollection() );

		void OnDestroy()
		{
			_destroyed = true;
			_onDestroy?.Trigger();
			_isAlive?.SetFalse();
			_lifetimeValidator?.Void();
		}

		void OnEnable() => _isActive?.SetTrue();
		void OnDisable() => _isActive?.SetFalse();
	}
	public static IUnityEventsRelay EventRelay( this GameObject go ) => go.RequestSibling<GameObjectEventsRelay>();
}