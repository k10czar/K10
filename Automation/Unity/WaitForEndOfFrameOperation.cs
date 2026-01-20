using System.Collections;
using UnityEngine;

namespace K10.Automation.Unity
{
	public class WaitForEndOfFrameOperation : Automation.BaseOperation
	{
		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{ 
			yield return new UnityEngine.WaitForEndOfFrame(); 
		}

		public override string ToString() => $"🖼 {"WaitForEndOfFrameOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
