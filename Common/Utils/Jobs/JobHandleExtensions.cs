using System.Runtime.CompilerServices;
using Unity.Jobs;

public static class JobHandleExtensions
{
#if CODE_METRICS
	const string METRICS_CODE_TAG = "JobHandle.Complete";
#endif

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static void TryMesureComplete(this JobHandle handle)
	{
#if CODE_METRICS
		CodeTimingDebug.LogStart(METRICS_CODE_TAG);
#endif
		handle.Complete();
#if CODE_METRICS
		CodeTimingDebug.LogEnd(METRICS_CODE_TAG);
#endif
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static void TryMesureCompleteWithTag(this JobHandle handle, string tag)
	{
#if CODE_METRICS
		CodeTimingDebug.LogStart(tag);
#endif
		handle.Complete();
#if CODE_METRICS
		CodeTimingDebug.LogEnd(tag);
#endif
	}
}
