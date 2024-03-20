using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class WaitForEndOfFrameOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine() 
		{ 
			yield return new UnityEngine.WaitForEndOfFrame(); 
		}

		public string GetSummaryColored() => $"🖼 {"WaitForEndOfFrameOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
