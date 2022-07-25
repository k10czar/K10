using System.Collections;
using UnityEngine;

namespace Unity.Automation
{
	public class DebugBreakOperation : K10.Automation.Operation
	{
		public override IEnumerator ExecutionCoroutine()
		{
			Debug.Break();
			if( false ) yield return null;
		}
	}
}
