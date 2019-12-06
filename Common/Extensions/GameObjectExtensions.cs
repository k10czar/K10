using UnityEngine;

public static class GameObjectExtensions
{
	public interface IUnityEventsRelay
	{
		IEventRegister OnDestroy { get; }
	}

	public class GameObjectEventsRelay : MonoBehaviour, IUnityEventsRelay
	{
		private readonly EventSlot _onDestroy = new EventSlot();
		IEventRegister IUnityEventsRelay.OnDestroy => _onDestroy;

		void OnDestroy() => _onDestroy.Trigger();
	}

	public static IUnityEventsRelay EventRelay( this GameObject go ) => go.RequestSibling<GameObjectEventsRelay>();
}