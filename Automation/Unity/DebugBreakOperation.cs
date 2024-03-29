using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
    public class DebugBreakOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine()  
		{
			Debug.Break();
			if( false ) yield return null;
		}

		public string GetSummaryColored() => $"💥 {"DebugBreakOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
