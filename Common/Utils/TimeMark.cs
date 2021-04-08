public struct TimeMark
{
	private float _lastMark;

	public TimeMark( float initialMark ) { _lastMark = initialMark; }
	public void Reset() { _lastMark = UnityEngine.Time.unscaledTime; }
	public bool Passed( float seconds ) => _lastMark + seconds < UnityEngine.Time.unscaledTime;
}
