using System.Collections;
using System;
using UnityEngine;

namespace Automation.Unity
{
	public class GcCollectOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine() 
		{
			GC.Collect();
			yield return null;
		}

		public string GetSummaryColored() => $"🗑 {"GcCollectOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
