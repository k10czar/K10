using UnityEngine;
public static class GameObjectExtensions
{
	public static IUnityEventsRelay EventRelay( this GameObject go ) => go.RequestSibling<GameObjectEventsRelay>();
}
