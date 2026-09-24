using System;
using System.Collections;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace K10
{
	[NoAutoStaticsCleanup]
	public class ExternalCoroutine : MonoBehaviour
	{
		private static ExternalCoroutine _instance;

		public static Coroutine Play(IEnumerator coroutine, ref Coroutine cacheVariable)
		{
			Debug.Assert(cacheVariable == null, "Overwriting coroutine cache variable");

			cacheVariable = Play(coroutine);
			return cacheVariable;
		}

		public static Coroutine Play(IEnumerator coroutine) => _instance.StartCoroutine(coroutine);

		public static IEnumerator AwaitAll(params IEnumerator[] routines)
		{
			var coroutines = new Coroutine[routines.Length];

			for (var index = 0; index < routines.Length; index++)
			{
				var routine = routines[index];
				if (routine == null) continue;

				coroutines[index] = _instance.StartCoroutine(routine);
			}

			foreach (var routine in coroutines)
				yield return routine;
		}

		public static void Stop(ref Coroutine cacheVariable)
		{
			if (cacheVariable == null) return;

			Stop(cacheVariable);
			cacheVariable = null;
		}

		public static void Stop(Coroutine coroutine) => _instance.StopCoroutine(coroutine);

		public static Coroutine DelayedCall(Action callback, float delay) => Play(DelayedCallCoroutine(callback, delay));

		private static IEnumerator DelayedCallCoroutine(Action callback, float delay)
		{
			yield return new WaitForSeconds(delay);
			callback.Invoke();
		}

		public static Coroutine CallNextFrame(Action callback) => Play(CallNextFrameCoroutine(callback));

		private static IEnumerator CallNextFrameCoroutine(Action callback)
		{
			yield return new WaitForEndOfFrame();
			callback.Invoke();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			if (_instance != null)
				Destroy(_instance.gameObject);

			var go = new GameObject("External Coroutine");
			DontDestroyOnLoad(go);

			_instance = go.AddComponent<ExternalCoroutine>();
		}
	}
}