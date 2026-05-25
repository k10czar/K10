using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

public static class ProfilerUtils
{
	public const int DEFAULT_FRAMES_CAPTURE = 100;

	public static void CaptureFrames( int frames = DEFAULT_FRAMES_CAPTURE, bool clean = true, bool log = false )
	{
		ExternalCoroutine.StartCoroutine( CaptureFramesCoroutine( frames, clean, log ) );
	}

	public static IEnumerator CaptureFramesCoroutine( int frames = DEFAULT_FRAMES_CAPTURE, bool clean = true, bool log = false ) 
	{ 
#if UNITY_EDITOR
		StartCapture( clean );
		if( log ) Debug.Log($"Profiler will record {frames} frames");
		for( int i = frames; i > 0; i-- ) yield return null;
		StopCapture();
		if( log ) Debug.Log($"Profiler has recorded {frames} frames");
#else
		if( log ) Debug.Log("Profiler operations only work in Editor.");
		yield return null;
#endif
	}

	public static void Clear()
	{
#if UNITY_EDITOR
		ProfilerDriver.ClearAllFrames();
#endif
	}

	public static void StartCapture( bool clean = true )
	{
#if UNITY_EDITOR
		ProfilerDriver.enabled = true;
		ProfilerDriver.enabled = true;
		ProfilerDriver.profileEditor = true;
		if( clean ) ProfilerDriver.ClearAllFrames();
#endif
	}

	public static void StopCapture()
	{
#if UNITY_EDITOR
		ProfilerDriver.enabled = false;
#endif
	}
}
