using System.Collections;


namespace Unity.Automation
{
	public class WaitForEndOfFrameOperation : K10.Automation.Operation
	{
		public override IEnumerator ExecutionCoroutine() { yield return new UnityEngine.WaitForEndOfFrame(); }
	}
}
