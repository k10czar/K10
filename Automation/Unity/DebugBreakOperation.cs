using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
    public class DebugBreakOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine( bool log = false )
		{
			Debug.Break();

#pragma warning disable CS0162 // Unreachable code detected
			if( false ) yield return null;
#pragma warning restore CS0162 // Unreachable code detected
		}

		public override string ToString() => $"💥 {"DebugBreakOperation".Colorfy( Colors.Console.Verbs )}";
	}
}