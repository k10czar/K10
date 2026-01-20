using System.Collections;
using System;
using UnityEngine;

namespace K10.Automation.Unity
{
	public class GcCollectOperation : Automation.BaseOperation
	{
		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			GC.Collect();
			yield return null;
		}

		public override string ToString() => $"🗑 {"GcCollectOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
