using System.Collections;
using UnityEngine;

public class ExternalCoroutine : MonoBehaviour
{
	private static ExternalCoroutine instance = null;
	public static ExternalCoroutine Instance => instance;

	private static void TryCreateInstance()
	{
		if( instance == null )
		{
			var go = new GameObject( "External Coroutine" );
			Object.DontDestroyOnLoad( go );

			instance = go.AddComponent<ExternalCoroutine>();
		}
	}

	public static new Coroutine StartCoroutine( IEnumerator coroutine )
	{
		TryCreateInstance();
		return instance.BaseStartCoroutine( coroutine );
	}

	public static new void StopCoroutine( Coroutine coroutine )
	{
		TryCreateInstance();
		instance.BaseStopCoroutine( coroutine );
	}

	private Coroutine BaseStartCoroutine( IEnumerator coroutine )
	{
		return base.StartCoroutine( coroutine );
	}

	private void BaseStopCoroutine( Coroutine coroutine )
	{
		base.StopCoroutine( coroutine );
	}
}
