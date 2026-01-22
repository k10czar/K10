using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace K10.Automation
{
    [ListingPath("Unity/Profiler/Start")]
	public class StartProfiler : BaseOperation
    {
		public override string EmojiIcon => "🔴";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{ 
#if UNITY_EDITOR
            ProfilerDriver.enabled = true;
            ProfilerDriver.enabled = true;
            ProfilerDriver.profileEditor = true;
            ProfilerDriver.ClearAllFrames();
            if( log ) Debug.Log($"Profiler start");
#else
            if( log ) Debug.Log("Profiler operations only work in Editor.");
#endif
            yield return null;
		}
    }
}