using UnityEngine;
public static class GameObjectExtensions
{
	public interface IUnityEventsRelay
	{
		IEventRegister OnDestroy { get; }
		IBoolStateObserver IsActive { get; }
		IBoolStateObserver IsAlive { get; }
	}
	public class GameObjectEventsRelay : MonoBehaviour, IUnityEventsRelay
	{
		private readonly EventSlot _onDestroy = new EventSlot();
		private readonly BoolState _isActive = new BoolState();
		private readonly BoolState _isAlive = new BoolState( true );

		IEventRegister IUnityEventsRelay.OnDestroy => _onDestroy;
		public IEventRegister OnDestroyEvent => _onDestroy;
		public IBoolStateObserver IsActive => _isActive;
		public IBoolStateObserver IsAlive => _isAlive;

		void OnDestroy()
		{
			_onDestroy.Trigger();
			_isAlive.SetFalse();
		}

		void OnEnable() => _isActive.SetTrue();
		void OnDisable() => _isActive.SetFalse();
	}
	public static IUnityEventsRelay EventRelay( this GameObject go ) => go.RequestSibling<GameObjectEventsRelay>();
}