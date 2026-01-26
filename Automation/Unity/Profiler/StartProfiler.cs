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
            ProfilerUtils.StartCapture();
            yield return null;
		}
    }
}