using UnityEngine;
public static class GameObjectExtensions
{
	public interface IUnityEventsRelay
	{
		IEventRegister OnDestroy { get; }
		IBoolStateObserver IsActive { get; }
	}
	public class GameObjectEventsRelay : MonoBehaviour, IUnityEventsRelay
	{
		private readonly EventSlot _onDestroy = new EventSlot();
		private readonly BoolState _isActive = new BoolState();

		IEventRegister IUnityEventsRelay.OnDestroy => _onDestroy;
		public IEventRegister OnDestroyEvent => _onDestroy;
		public IBoolStateObserver IsActive => _isActive;
		void OnDestroy() => _onDestroy.Trigger();
		void OnEnable() => _isActive.SetTrue();
		void OnDisable() => _isActive.SetFalse();
	}
	public static IUnityEventsRelay EventRelay( this GameObject go ) => go.RequestSibling<GameObjectEventsRelay>();
}