using System.Collections;
using UnityEngine;

namespace Unity.Automation
{
	public class WaitForSecondsOperation : K10.Automation.Operation
	{
		[SerializeField] float _seconds;

		public override IEnumerator ExecutionCoroutine() { yield return new WaitForSecondsRealtime( _seconds ); }
	}
}
