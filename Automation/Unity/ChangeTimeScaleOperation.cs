

namespace Unity.Automation
{
	public class ChangeTimeScaleOperation : K10.Automation.Operation
	{
		[UnityEngine.SerializeField] float _timeScale = 1;

		public override System.Collections.IEnumerator ExecutionCoroutine() { UnityEngine.Time.timeScale = _timeScale; yield return null; }
	}
}
