using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace K10.Automation
{

	[ListingPath("Unity/Profiler/Frames")]
	public class ProfileFrames : BaseOperation
    {
#if UNITY_2023_1_OR_NEWER
		[ExtendedDrawer, SerializeReference] IValueProvider<int> _framesRef;
#else
		[SerializeField] int _framesVal = 10;
#endif

		public int Frames =>	
#if UNITY_2023_1_OR_NEWER
			_framesRef?.Value ?? 10;
#else
			_framesVal;
#endif

		public override string EmojiIcon => "📊";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{ 
#if UNITY_EDITOR
            ProfilerDriver.enabled = true;
            ProfilerDriver.enabled = true;
            ProfilerDriver.profileEditor = true;
            ProfilerDriver.ClearAllFrames();
            
            var frames = Frames;
            if( log ) Debug.Log($"Profiler will record {frames} frames");
			for( int i = frames; i > 0; i-- ) yield return null;

            ProfilerDriver.enabled = false;
            if( log ) Debug.Log($"Profiler has recorded {frames} frames");
#else
            if( log ) Debug.Log("Profiler operations only work in Editor.");
            yield return null;
#endif
		}

		public override string ToString() => $"{base.ToString()} for {Frames}";
    }
}