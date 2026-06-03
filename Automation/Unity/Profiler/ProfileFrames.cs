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
		[ExtendedDrawer, SerializeReference] IValueProvider<bool> _cleanRef;
#else
		[SerializeField] int _framesVal = 10;
		[SerializeField] bool _cleanVal = false;
#endif

		public int Frames =>	
#if UNITY_2023_1_OR_NEWER
			_framesRef?.Value ?? 10;
#else
			_framesVal;
#endif

		public bool Clean =>	
#if UNITY_2023_1_OR_NEWER
			_cleanRef?.Value ?? false;
#else
			_cleanVal;
#endif

		public override string EmojiIcon => "📊";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{ 
			yield return ProfilerUtils.CaptureFramesCoroutine( Frames, Clean, log );
		}

		public override string ToString() => $"{base.ToString()} for {Frames}";
    }
}