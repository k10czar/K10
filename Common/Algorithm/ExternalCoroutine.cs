using System;
using System.Collections;
using UnityEngine;

//namespace K10
//{
	public class ExternalCoroutine : MonoBehaviour
	{
		private static ExternalCoroutine instance = null;
		public static ExternalCoroutine Instance 
		{
			get
			{
				TryCreateInstance();
				return instance;
			}
		}

		private static void TryCreateInstance()
		{
			if (instance != null) return;

			var go = new GameObject("External Coroutine");
			DontDestroyOnLoad(go);

			instance = go.AddComponent<ExternalCoroutine>();
		}

		public static Coroutine Play(IEnumerator coroutine, ref Coroutine cacheVariable)
		{
			Debug.Assert(cacheVariable == null, "Overwriting coroutine cache variable");

			cacheVariable = Play(coroutine);
			return cacheVariable;
		}

		public static Coroutine Play(IEnumerator coroutine)
		{
			TryCreateInstance();
			return instance.StartCoroutine(coroutine);
		}

		public static void Stop(ref Coroutine cacheVariable)
		{
			Stop(cacheVariable);
			cacheVariable = null;
		}

		public static void Stop(Coroutine coroutine)
		{
			TryCreateInstance();
			instance.StopCoroutine(coroutine);
		}

		public static Coroutine DelayedCall(Action callback, float delay) => Play(DelayedCallCoroutine(callback, delay));

		private static IEnumerator DelayedCallCoroutine(Action callback, float delay)
		{
			yield return new WaitForSeconds(delay);
			callback.Invoke();
		}
	}
//}