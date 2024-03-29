using UnityEngine;

public static class TimeModification
{
	private static readonly ReferenceOverrideRequester<float> _timeScaleValue = new ReferenceOverrideRequester<float>( 1 );

	public static void Request( float value, object key ) { _timeScaleValue.RequestOverride( value, key ); UpdateTimeScale(); }
	public static void Remove( object key ) { _timeScaleValue.RemoveRequest( key ); UpdateTimeScale(); }
	public static void Clear() { _timeScaleValue.ClearOverrides(); UpdateTimeScale(); }

	private static void UpdateTimeScale()
	{
		if( !Mathf.Approximately( Time.timeScale, _timeScaleValue.CurrentReference ) )
		{
#if UNITY_EDITOR
			Debug.Log( $"<color=cyan>Change TimeScale</color> from <color=blue>{Time.timeScale}</color> to <color=blue>{_timeScaleValue.CurrentReference}</color> [ {_timeScaleValue} ]" );
#else
			Debug.Log( $"Change TimeScale from {Time.timeScale} to {_timeScaleValue.CurrentReference} [ {_timeScaleValue} ]" );
#endif
		}
		Time.timeScale = _timeScaleValue.CurrentReference;
	}
}
