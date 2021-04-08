public class TimeoutTracker
{
	private TimeMark _mark = new TimeMark( float.MinValue );
	private float _timeout = 0;

	public TimeoutTracker( float timeout ) { _timeout = timeout; }
	public void Reset() { _mark.Reset(); }
	public bool IsValid => !_mark.Passed( _timeout );
	public bool IsExpired => _mark.Passed( _timeout );
}
