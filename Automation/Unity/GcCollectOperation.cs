using System.Collections;
using System;

namespace Unity.Automation
{
	public class GcCollectOperation : K10.Automation.Operation
	{
		public override IEnumerator ExecutionCoroutine() 
		{
			GC.Collect();
			yield return null;
		}
	}
}
