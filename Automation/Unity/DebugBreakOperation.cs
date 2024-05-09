using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
    public class DebugBreakOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine( bool log = false )  
		{
			Debug.Break();
			if( false ) yield return null;
		}

		public override string ToString() => $"💥 {"DebugBreakOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
