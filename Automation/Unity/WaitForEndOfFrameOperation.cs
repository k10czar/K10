

namespace Unity.Automation
{
	public class WaitForEndOfFrameOperation : K10.Automation.Operation
	{
		public override System.Collections.IEnumerator ExecutionCoroutine() { yield return new UnityEngine.WaitForEndOfFrame(); }
	}
}
