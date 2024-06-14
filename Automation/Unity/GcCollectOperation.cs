using System.Collections;
using System;
using UnityEngine;

namespace Automation.Unity
{
	public class GcCollectOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			GC.Collect();
			yield return null;
		}

		public override string ToString() => $"🗑 {"GcCollectOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
