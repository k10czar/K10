using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace K10.Automation
{
    [ListingPath("Unity/Profiler/Start")]
	public class StopProfiler : BaseOperation
    {
		public override string EmojiIcon => "📊";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{ 
#if UNITY_EDITOR
            if( log ) Debug.Log($"Profiler stoped");
            ProfilerDriver.enabled = false;
#else
            if( log ) Debug.Log("Profiler operations only work in Editor.");
#endif
            yield return null;
		}
    }
}